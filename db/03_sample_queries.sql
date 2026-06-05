-- ============================================================================
--  AgroSmart - Consultas SQL de simulação de uso
--  Demonstram cadastro, consulta e análise das métricas ambientais e alertas.
-- ============================================================================

-- 1) Inventário: dispositivos por região com status
SELECT r.Code AS regiao, r.Name AS nome_regiao, d.Identifier AS dispositivo,
       d.DeviceType AS tipo, d.Status AS status
  FROM AGS_REGIONS r
  JOIN AGS_DEVICES d ON d.RegionId = r.Id
 ORDER BY r.Code, d.Identifier;

-- 2) Quantos dispositivos cada região possui
SELECT r.Code AS regiao, COUNT(d.Id) AS qtd_dispositivos
  FROM AGS_REGIONS r
  LEFT JOIN AGS_DEVICES d ON d.RegionId = r.Id
 GROUP BY r.Code
 ORDER BY qtd_dispositivos DESC;

-- 3) Última medição de cada métrica por região (snapshot de salubridade)
SELECT regiao, metrica, unidade, valor, coletado_em
  FROM (
        SELECT r.Code AS regiao, mt.Code AS metrica, mt.Unit AS unidade,
               m.Value AS valor, sr.CollectedAt AS coletado_em,
               ROW_NUMBER() OVER (PARTITION BY r.Id, mt.Id
                                  ORDER BY sr.CollectedAt DESC) AS rn
          FROM AGS_MEASUREMENTS m
          JOIN AGS_SENSOR_READINGS sr ON sr.Id = m.SensorReadingId
          JOIN AGS_DEVICES d          ON d.Id = sr.DeviceId
          JOIN AGS_REGIONS r          ON r.Id = d.RegionId
          JOIN AGS_METRIC_TYPES mt    ON mt.Id = m.MetricTypeId
       )
 WHERE rn = 1
 ORDER BY regiao, metrica;

-- 4) Medições FORA da faixa nominal (indicativo de risco)
SELECT r.Code AS regiao, d.Identifier AS dispositivo, mt.Code AS metrica,
       m.Value AS valor, mt.NominalMin, mt.NominalMax, sr.CollectedAt
  FROM AGS_MEASUREMENTS m
  JOIN AGS_SENSOR_READINGS sr ON sr.Id = m.SensorReadingId
  JOIN AGS_DEVICES d          ON d.Id = sr.DeviceId
  JOIN AGS_REGIONS r          ON r.Id = d.RegionId
  JOIN AGS_METRIC_TYPES mt    ON mt.Id = m.MetricTypeId
 WHERE (mt.NominalMin IS NOT NULL AND m.Value < mt.NominalMin)
    OR (mt.NominalMax IS NOT NULL AND m.Value > mt.NominalMax)
 ORDER BY sr.CollectedAt DESC;

-- 5) Relatório de alertas por região e severidade
SELECT r.Code AS regiao, a.Severity AS severidade, a.Status AS situacao,
       COUNT(*) AS total
  FROM AGS_ALERTS a
  JOIN AGS_REGIONS r ON r.Id = a.RegionId
 GROUP BY r.Code, a.Severity, a.Status
 ORDER BY r.Code, a.Severity;

-- 6) Alertas abertos mais recentes com contexto completo
SELECT a.Id, a.CreatedAt, a.Severity, a.Status, a.Message,
       r.Code AS regiao, d.Identifier AS dispositivo, mt.Code AS metrica,
       a.TriggeredValue AS valor_disparo
  FROM AGS_ALERTS a
  JOIN AGS_REGIONS r       ON r.Id = a.RegionId
  JOIN AGS_DEVICES d       ON d.Id = a.DeviceId
  JOIN AGS_METRIC_TYPES mt ON mt.Id = a.MetricTypeId
 WHERE a.Status = 'Open'
 ORDER BY a.CreatedAt DESC
 FETCH FIRST 20 ROWS ONLY;

-- 7) Métrica com mais alertas (padrão crítico)
SELECT mt.Code AS metrica, COUNT(*) AS qtd_alertas
  FROM AGS_ALERTS a
  JOIN AGS_METRIC_TYPES mt ON mt.Id = a.MetricTypeId
 GROUP BY mt.Code
 ORDER BY qtd_alertas DESC;

-- 8) Média / mínimo / máximo das métricas nas últimas 24h por região
SELECT r.Code AS regiao, mt.Code AS metrica,
       ROUND(AVG(m.Value), 2) AS media,
       ROUND(MIN(m.Value), 2) AS minimo,
       ROUND(MAX(m.Value), 2) AS maximo,
       COUNT(*) AS amostras
  FROM AGS_MEASUREMENTS m
  JOIN AGS_SENSOR_READINGS sr ON sr.Id = m.SensorReadingId
  JOIN AGS_DEVICES d          ON d.Id = sr.DeviceId
  JOIN AGS_REGIONS r          ON r.Id = d.RegionId
  JOIN AGS_METRIC_TYPES mt    ON mt.Id = m.MetricTypeId
 WHERE sr.CollectedAt >= SYSTIMESTAMP - INTERVAL '1' DAY
 GROUP BY r.Code, mt.Code
 ORDER BY r.Code, mt.Code;

-- 9) Dispositivos "silenciosos" (sem leituras nas últimas 6h) - possível falha
SELECT d.Identifier AS dispositivo, r.Code AS regiao, d.Status, d.LastSeenAt
  FROM AGS_DEVICES d
  JOIN AGS_REGIONS r ON r.Id = d.RegionId
 WHERE d.LastSeenAt IS NULL
    OR d.LastSeenAt < SYSTIMESTAMP - INTERVAL '6' HOUR
 ORDER BY d.LastSeenAt NULLS FIRST;

-- 10) Tempo médio de reconhecimento de alertas (Open -> Acknowledged)
SELECT ROUND(AVG(EXTRACT(DAY    FROM (a.AcknowledgedAt - a.CreatedAt)) * 24 * 60
                + EXTRACT(HOUR   FROM (a.AcknowledgedAt - a.CreatedAt)) * 60
                + EXTRACT(MINUTE FROM (a.AcknowledgedAt - a.CreatedAt))), 1) AS minutos_medios_ate_ack
  FROM AGS_ALERTS a
 WHERE a.AcknowledgedAt IS NOT NULL;
