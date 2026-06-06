# AgroSmart

Plataforma de **monitoramento ambiental e agricultura de precisão** para lavouras,
pastagens e estufas. A solução ingere telemetria de sensores de campo em **tempo
real** via **Apache Kafka**, persiste medições em **Oracle**, avalia **regras de
alerta** configuráveis e expõe operações e relatórios por uma **API REST** em
**ASP.NET Core 9**.

Evolução terrestre do ecossistema *Orbital Greenhouse*, com foco em pipeline de
streaming, containerização e operação em ambiente de produção simulado.

---

## Visão geral

O AgroSmart combina três capacidades principais:

| Capacidade | Descrição |
|------------|-----------|
| **Pipeline em streaming** | Leituras de sensores trafegam por Kafka; a API consome o fluxo, persiste e dispara alertas sem acoplamento HTTP entre produtor e consumidor. |
| **Containerização** | Kafka, interface de observabilidade, API e simulador de telemetria sobem integrados via Docker Compose. |
| **Gestão operacional** | REST/Swagger para talhões, dispositivos, regras, alertas, leituras e relatórios; autenticação JWT. |

Documentação complementar:

- Pipeline Kafka e arquitetura Docker — [`docs/PIPELINE.md`](docs/PIPELINE.md)
- Modelo de negócio (Business Model Canvas) — [`docs/BUSINESS_MODEL_CANVAS.md`](docs/BUSINESS_MODEL_CANVAS.md)
- Diagrama ER e coleção Postman — [`docs/ER_DIAGRAM.md`](docs/ER_DIAGRAM.md), [`docs/POSTMAN.md`](docs/POSTMAN.md)

---

## Arquitetura

```
Simulator ──(Kafka)──> API Consumer ──> IngestionService ──> Oracle (AGS_*)
 (producer)   topic        (BackgroundService)   (regras de alerta)     +  REST/Swagger
```

- **Camadas da API:** `Controller → Service → Repository → DbContext` (EF Core / Oracle).
- **Producer:** `AgroSmart.Simulator` publica JSON no tópico `agrosmart.sensor-readings` (~3 s entre leituras).
- **Consumer:** `SensorReadingConsumer` (`BackgroundService`) lê o tópico, delega à `IngestionService` e registra alertas quando limites são violados.
- **Observabilidade:** Kafka UI expõe tópicos, mensagens e consumer groups.

### Estrutura do repositório

```
AgroSmart/
├── AgroSmart.sln
├── AgroSmart.Api/                # API .NET 9 (REST, EF/Oracle, consumer Kafka)
│   ├── Messaging/                # KafkaSettings, SensorReadingConsumer
│   ├── Controllers/ Services/ Repositories/ Data/ Models/ Dtos/ Migrations/
│   └── Dockerfile
├── AgroSmart.Simulator/          # Producer Kafka (telemetria simulada)
├── AgroSmart.Api.Tests/          # Testes unitários (xUnit)
├── db/                           # Scripts SQL (DDL, seed, reset, consultas)
├── docs/                         # Pipeline, Canvas, ER, Postman
├── docker-compose.yml            # kafka, kafka-ui, api, simulator
└── .github/workflows/            # Publicação de imagens no Docker Hub
```

---

## Stack tecnológica

| Camada | Tecnologia |
|--------|------------|
| API | .NET 9, ASP.NET Core Web API, Swagger/OpenAPI |
| Persistência | Entity Framework Core 9, Oracle (`Oracle.EntityFrameworkCore`) |
| Streaming | Apache Kafka 3.7 (KRaft), `Confluent.Kafka` |
| Segurança | JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`), hash de senha (PBKDF2) |
| Infraestrutura | Docker, Docker Compose, GitHub Actions |

---

## Modelo de dados

Tabelas Oracle com prefixo `AGS_`, projetadas para coexistir com outros projetos no
mesmo schema institucional.

| Tabela | Descrição |
|--------|-----------|
| `AGS_REGIONS` | Talhões e áreas de cultivo |
| `AGS_METRIC_TYPES` | Catálogo de métricas (unidade e faixa nominal) |
| `AGS_DEVICES` | Sensores vinculados a um talhão |
| `AGS_SENSOR_READINGS` | Evento de coleta |
| `AGS_MEASUREMENTS` | Valores individuais de cada leitura |
| `AGS_ALERT_RULES` | Limiares configuráveis (min/max) |
| `AGS_ALERTS` | Alertas automáticos ou manuais |
| `AGS_USERS` | Operadores autenticados |

O schema pode ser aplicado por **migrations EF Core** (ambiente Development) ou pelos
scripts em [`db/`](db/). Constraints e índices usam prefixo `AGS_` para evitar
conflito de nomes no Oracle compartilhado.

---

## Pipeline Kafka

| Elemento | Valor / papel |
|----------|----------------|
| Tópico | `agrosmart.sensor-readings` |
| Consumer group | `agrosmart-api` |
| Broker (Docker) | `kafka:9092` (rede interna) · `localhost:29092` (host) |
| Dispositivos simulados | `SENSOR-T1-01`, `SENSOR-T2-01`, `SENSOR-E1-01` |

Payload típico de uma mensagem:

```json
{
  "deviceIdentifier": "SENSOR-T1-01",
  "collectedAt": "2026-06-05T18:30:00Z",
  "measurements": [
    { "metricCode": "TEMPERATURE", "value": 35.2 },
    { "metricCode": "SOIL_MOISTURE", "value": 24.0 }
  ]
}
```

Detalhes de brokers, partições, offsets e containerização: [`docs/PIPELINE.md`](docs/PIPELINE.md).

---

## Segurança

- Senhas armazenadas como **hash** (nunca em texto puro).
- Endpoints protegidos exigem `Authorization: Bearer {token}`.
- Validação de entrada via Data Annotations nos DTOs.
- Acesso a dados exclusivamente por **EF Core** (consultas parametrizadas).

---

## Configuração

### Banco Oracle

| Variável / arquivo | Uso |
|--------------------|-----|
| `ConnectionStrings:OracleDb` | Connection string completa |
| `appsettings.Development.local.json` | Override local (não versionado) |
| `.env` → `ORACLE_USER`, `ORACLE_PASSWORD` | Credenciais no Docker Compose |

Exemplo de connection string:

```
User Id=RM97674;Password=***;Data Source=oracle.fiap.com.br:1521/ORCL
```

### Kafka (API)

| Chave | Padrão | Descrição |
|-------|--------|-----------|
| `Kafka:Enabled` | `false` (local) / `true` (Docker) | Ativa o `SensorReadingConsumer` |
| `Kafka:BootstrapServers` | `localhost:9092` / `kafka:9092` | Endereço do broker |
| `Kafka:Topic` | `agrosmart.sensor-readings` | Tópico de leituras |
| `Kafka:GroupId` | `agrosmart-api` | Consumer group |

### Simulador

| Variável | Padrão | Descrição |
|----------|--------|-----------|
| `KAFKA_BOOTSTRAP_SERVERS` | `localhost:9092` | Broker |
| `KAFKA_TOPIC` | `agrosmart.sensor-readings` | Tópico de destino |
| `SIMULATOR_INTERVAL_MS` | `3000` | Intervalo entre publicações |

### Docker Hub (opcional)

| Variável / secret | Descrição |
|-------------------|-----------|
| `DOCKERHUB_USERNAME` (`.env`) | Prefixo das imagens locais |
| `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN` (GitHub Actions) | Publicação automática no push para `main` |

---

## Execução

### Docker Compose (ambiente completo)

Sobe Kafka, Kafka UI, API e simulador. Requer Docker e arquivo `.env` derivado de
[`.env.example`](.env.example).

```bash
docker compose up --build -d
```

| Serviço | URL |
|---------|-----|
| API / Swagger | http://localhost:8080/swagger |
| Kafka UI | http://localhost:8081 |

Logs do pipeline:

```bash
docker compose logs -f simulator
docker compose logs -f api
```

### Execução local (sem Compose completo)

A API pode rodar com `dotnet run --project AgroSmart.Api`. O consumer Kafka permanece
desabilitado por padrão (`Kafka:Enabled=false` em `appsettings.json`). Para ativar o
streaming localmente, é necessário um broker acessível e `Kafka:Enabled=true`.

O simulador publica no tópico configurado via `dotnet run --project AgroSmart.Simulator`.

### Reset do schema Oracle

O script [`db/00_drop_ags_schema.sql`](db/00_drop_ags_schema.sql) remove tabelas `AGS_*`
e o histórico EF. Após executá-lo, a API em Development recria o schema e os dados
de demonstração na próxima inicialização.

---

## API REST

### Autenticação (ambiente Development)

| Campo | Valor |
|-------|-------|
| E-mail | `operador@agrosmart.com.br` |
| Senha | `agrosmart123` |
| Papel | `Operator` |

O operador e os sensores de demonstração são criados automaticamente pelo seeder em
Development. Login: `POST /api/v1/auth/login`.

### Recursos principais

| Recurso | Rota base |
|---------|-----------|
| Auth | `POST /api/v1/auth/register` · `POST /api/v1/auth/login` |
| Regions | `GET/POST/PUT/DELETE /api/v1/regions` |
| Devices | `GET/POST/PUT/DELETE /api/v1/devices` |
| Metric Types | `GET/POST/PUT/DELETE /api/v1/metric-types` |
| Alert Rules | `GET/POST/PUT/DELETE /api/v1/alert-rules` |
| Ingestion (HTTP) | `POST /api/v1/ingestion/readings` · `/readings/batch` · `/upload` |
| Alerts | `GET /api/v1/alerts` · `PUT /{id}/status` |
| Readings | `GET /api/v1/readings/{id}` · `/by-device/{deviceId}` |
| Reports | `GET /api/v1/reports/region-health` · `/alerts-summary` |
| Health | `GET /api/healthcheck` · `/api/healthcheck/full` |

A ingestão em tempo real via Kafka é processada pelo `SensorReadingConsumer` em
paralelo aos endpoints HTTP de ingestão.

---

## Testes

```bash
dotnet test
```

A suíte `AgroSmart.Api.Tests` cobre avaliação de limiares, unicidade de talhão,
integridade referencial e cenários de autenticação, sem dependência de Oracle
(mocks e lógica isolada).

---

## Licença

MIT — ver [`LICENSE`](LICENSE).
