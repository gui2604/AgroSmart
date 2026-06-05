-- ============================================================================
--  AgroSmart - Dados de exemplo para simulação de uso
--  Execute APÓS 01_create_tables.sql.
--
--  Observação: se o schema foi criado via EF Core (dotnet ef database update),
--  as 8 métricas em AGS_METRIC_TYPES já são inseridas pelo seed do EF. Nesse
--  caso, pule a seção 1 deste script para evitar violação de UX_METRIC_CODE.
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 1) Catálogo de métricas ambientais
-- ---------------------------------------------------------------------------
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('TEMPERATURE',  'Air Temperature',      'C',         'Temperatura do ar no módulo de cultivo.',          18,   26);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('HUMIDITY',     'Relative Humidity',    '%',         'Umidade relativa do ar.',                          50,   80);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('CO2',          'Carbon Dioxide',       'ppm',       'Concentração de CO2 para fotossíntese.',          400, 1200);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('O2',           'Oxygen',               '%',         'Concentração de oxigênio na atmosfera da cabine.', 19,   23);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('LUMINOSITY',   'Photosynthetic Light', 'umol/m2/s', 'Radiação fotossinteticamente ativa.',             200,  800);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('SOIL_MOISTURE','Substrate Moisture',   '%',         'Umidade do substrato de cultivo.',                 40,   70);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('PH',           'Nutrient Solution pH', 'pH',        'Acidez da solução nutritiva hidropônica.',        5.5,  6.5);
INSERT INTO AGS_METRIC_TYPES (Code, Name, Unit, Description, NominalMin, NominalMax) VALUES ('EC',           'Nutrient Conductivity','mS/cm',     'Condutividade elétrica (força nutritiva).',       1.2,  2.4);

-- ---------------------------------------------------------------------------
-- 2) Regiões (talhões / áreas de cultivo)
-- ---------------------------------------------------------------------------
INSERT INTO AGS_REGIONS (Code, Name, ModuleType, FieldLocation, Description) VALUES ('TALHAO-01', 'Talhão Norte - Soja',    'Sequeiro', 'Fazenda Boa Vista / Setor Norte', 'Talhão de soja em sistema de sequeiro.');
INSERT INTO AGS_REGIONS (Code, Name, ModuleType, FieldLocation, Description) VALUES ('TALHAO-02', 'Talhão Sul - Milho',     'Irrigado', 'Fazenda Boa Vista / Setor Sul',   'Talhão de milho com pivô central.');
INSERT INTO AGS_REGIONS (Code, Name, ModuleType, FieldLocation, Description) VALUES ('ESTUFA-01', 'Estufa 01 - Hortaliças', 'Estufa',   'Fazenda Boa Vista / Estufas',     'Estufa de hortaliças folhosas com controle climático.');

-- ---------------------------------------------------------------------------
-- 3) Dispositivos / sensores de campo (vinculados às regiões pelo Id)
-- ---------------------------------------------------------------------------
INSERT INTO AGS_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-T1-01', 'Estação de Campo T1-01', 'MultiSensor', 'Active', '2.1.0', (SELECT Id FROM AGS_REGIONS WHERE Code = 'TALHAO-01'));
INSERT INTO AGS_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-T2-01', 'Estação de Campo T2-01', 'MultiSensor', 'Active', '2.1.0', (SELECT Id FROM AGS_REGIONS WHERE Code = 'TALHAO-02'));
INSERT INTO AGS_DEVICES (Identifier, Name, DeviceType, Status, FirmwareVersion, RegionId)
    VALUES ('SENSOR-E1-01', 'Sonda de Estufa E1-01',  'Soil Probe',  'Active', '2.0.3', (SELECT Id FROM AGS_REGIONS WHERE Code = 'ESTUFA-01'));

-- ---------------------------------------------------------------------------
-- 4) Regras de alerta (limiares agronômicos)
-- ---------------------------------------------------------------------------
INSERT INTO AGS_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('Temperatura alta (global)',    'Temperatura do ar acima do ideal para a cultura.',           (SELECT Id FROM AGS_METRIC_TYPES WHERE Code='TEMPERATURE'),   NULL, NULL, 32,  'Warning',  1);
INSERT INTO AGS_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('Umidade do ar baixa (global)', 'Umidade relativa do ar muito baixa.',                        (SELECT Id FROM AGS_METRIC_TYPES WHERE Code='HUMIDITY'),      NULL, 40,   NULL, 'Warning',  1);
INSERT INTO AGS_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('Umidade do solo crítica',      'Umidade do solo abaixo do nível seguro (estresse hídrico).', (SELECT Id FROM AGS_METRIC_TYPES WHERE Code='SOIL_MOISTURE'), NULL, 35,   NULL, 'Critical', 1);
INSERT INTO AGS_ALERT_RULES (Name, Description, MetricTypeId, RegionId, MinThreshold, MaxThreshold, Severity, IsActive)
    VALUES ('pH do solo fora de faixa',     'Acidez do solo fora da faixa agronômica ideal.',             (SELECT Id FROM AGS_METRIC_TYPES WHERE Code='PH'),            NULL, 5.5,  6.8, 'Warning',  1);

-- Usuário: o hash de senha é gerado pela API (ASP.NET Core Identity PasswordHasher).
-- Crie o operador via POST /api/v1/auth/register em vez de inserir aqui.

COMMIT;
