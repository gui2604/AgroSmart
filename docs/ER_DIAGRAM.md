# AgroSmart — Diagrama de Entidade-Relacionamento

Banco de dados relacional (Oracle) que sustenta o monitoramento ambiental e a
gestão de alertas da produção de alimentos no espaço.

## Diagrama (imagem)

![Diagrama ER](ER_DIAGRAM.png)

> Exportação PNG gerada a partir de [`er-diagram.mmd`](er-diagram.mmd). Para editar, altere o `.mmd` e regenere o PNG.

## Diagrama (Mermaid)

```mermaid
erDiagram
    AGS_REGIONS ||--o{ AGS_DEVICES : "possui"
    AGS_REGIONS ||--o{ AGS_ALERT_RULES : "escopo (opcional)"
    AGS_REGIONS ||--o{ AGS_ALERTS : "localiza"
    AGS_METRIC_TYPES ||--o{ AGS_MEASUREMENTS : "tipifica"
    AGS_METRIC_TYPES ||--o{ AGS_ALERT_RULES : "monitora"
    AGS_METRIC_TYPES ||--o{ AGS_ALERTS : "refere"
    AGS_DEVICES ||--o{ AGS_SENSOR_READINGS : "coleta"
    AGS_DEVICES ||--o{ AGS_ALERTS : "origina"
    AGS_SENSOR_READINGS ||--o{ AGS_MEASUREMENTS : "contém"
    AGS_MEASUREMENTS ||--o{ AGS_ALERTS : "dispara"
    AGS_ALERT_RULES ||--o{ AGS_ALERTS : "gera"
    AGS_USERS ||--o{ AGS_ALERTS : "reconhece"

    AGS_REGIONS {
        int Id PK
        string Code UK
        string Name
        string ModuleType
        string FieldLocation
        string Description
        timestamp CreatedAt
    }

    AGS_METRIC_TYPES {
        int Id PK
        string Code UK
        string Name
        string Unit
        string Description
        double NominalMin
        double NominalMax
    }

    AGS_DEVICES {
        int Id PK
        string Identifier UK
        string Name
        string DeviceType
        string Status
        string FirmwareVersion
        timestamp InstalledAt
        timestamp LastSeenAt
        int RegionId FK
    }

    AGS_SENSOR_READINGS {
        int Id PK
        timestamp CollectedAt
        timestamp ReceivedAt
        string SourceFile
        int DeviceId FK
    }

    AGS_MEASUREMENTS {
        int Id PK
        double Value
        int SensorReadingId FK
        int MetricTypeId FK
    }

    AGS_ALERT_RULES {
        int Id PK
        string Name
        string Description
        int MetricTypeId FK
        int RegionId FK
        double MinThreshold
        double MaxThreshold
        string Severity
        number IsActive
        timestamp CreatedAt
    }

    AGS_ALERTS {
        int Id PK
        string Message
        string Severity
        string Status
        double TriggeredValue
        timestamp CreatedAt
        timestamp AcknowledgedAt
        timestamp ResolvedAt
        int AlertRuleId FK
        int MetricTypeId FK
        int MeasurementId FK
        int DeviceId FK
        int RegionId FK
        int AcknowledgedByUserId FK
    }

    AGS_USERS {
        int Id PK
        string Email UK
        string PasswordHash
        string Role
        timestamp CreatedAt
    }
```

## Relacionamentos (cardinalidade)

| Relação | Tipo | Regra de negócio |
|---------|------|------------------|
| Region → Device | 1:N | Cada dispositivo pertence a uma região; uma região tem vários dispositivos. |
| Device → SensorReading | 1:N | Cada leitura (arquivo JSON) é enviada por um dispositivo. |
| SensorReading → Measurement | 1:N | Uma leitura agrupa várias medições de métricas. |
| MetricType → Measurement | 1:N | Cada medição é de um tipo de métrica do catálogo. |
| MetricType → AlertRule | 1:N | Regras monitoram uma métrica específica. |
| Region → AlertRule | 1:N (opcional) | Regra pode ser global (RegionId nulo) ou de uma região. |
| AlertRule → Alert | 1:N | Uma regra violada gera vários alertas ao longo do tempo. |
| Measurement → Alert | 1:N (opcional) | O alerta aponta a medição que o disparou (nulo se manual). |
| Device → Alert | 1:N | O alerta registra o dispositivo de origem. |
| Region → Alert | 1:N | O alerta registra a região afetada. |
| User → Alert | 1:N (opcional) | Operador que reconheceu/resolveu o alerta. |

## Regras de integridade

- `AGS_SENSOR_READINGS` e `AGS_MEASUREMENTS` usam **ON DELETE CASCADE**: apagar
  uma leitura remove suas medições; apagar um dispositivo remove suas leituras.
- `AGS_ALERTS.MeasurementId` e `AGS_ALERTS.AcknowledgedByUserId` usam
  **ON DELETE SET NULL** para preservar o histórico de alertas.
- Demais chaves estrangeiras usam **RESTRICT** (Oracle não permite múltiplos
  caminhos de cascata para a mesma tabela).
