# AgroSmart

API RESTful em **ASP.NET Core 9 (C#)** para **monitoramento ambiental e agricultura
de precisão** no campo (lavouras, pastagens e estufas). Sensores de campo coletam
métricas ambientais (temperatura, umidade do ar, umidade do solo, pH, luminosidade,
etc.) e transmitem leituras que são ingeridas em **tempo real via Apache Kafka**. O
servidor persiste as medições no **Oracle**, avalia **regras de alerta** configuráveis
e gera relatórios de salubridade por talhão.

> Evolução terrestre do projeto *Orbital Greenhouse*. Esta entrega corresponde à
> **Fase 4** do AgroSmart: **pipeline de dados em streaming**, **containerização com
> Docker** e **modelo de negócio (Business Model Canvas)**.

---

## Entregáveis da Fase 4

| # | Requisito | Onde está |
|---|-----------|-----------|
| 1 | **Pipeline de dados com streaming** (Kafka) | `AgroSmart.Simulator/` (producer) + `AgroSmart.Api/Messaging/` (consumer) + [`docs/PIPELINE.md`](docs/PIPELINE.md) |
| 2 | **Containerização** (Docker) | [`docker-compose.yml`](docker-compose.yml) + `*/Dockerfile` |
| 3 | **Modelo de negócio (Canvas)** | [`docs/BUSINESS_MODEL_CANVAS.md`](docs/BUSINESS_MODEL_CANVAS.md) |

> Os vídeos pedidos no enunciado serão gravados na finalização e não fazem parte deste repositório.

---

## Arquitetura

```
Simulator ──(Kafka)──> API Consumer ──> IngestionService ──> Oracle (AGS_*)
 (producer)   topic        (BackgroundService)   (regras de alerta)     +  REST/Swagger
```

- **Camadas:** `Controller → Service → Repository → DbContext (EF Core / Oracle)`.
- **Streaming:** o `AgroSmart.Simulator` publica leituras no tópico
  `agrosmart.sensor-readings`; o `SensorReadingConsumer` (na API) consome e ingere.
- **Detalhe completo do pipeline e diagramas:** [`docs/PIPELINE.md`](docs/PIPELINE.md).

### Estrutura do repositório

```
AgroSmart/
├── AgroSmart.sln
├── AgroSmart.Api/                # API .NET 9 (controllers, services, repos, EF/Oracle)
│   ├── Messaging/                # KafkaSettings + SensorReadingConsumer (consumer do pipeline)
│   ├── Controllers/ Services/ Repositories/ Data/ Models/ Dtos/ Migrations/ ...
│   └── Dockerfile
├── AgroSmart.Simulator/          # Producer Kafka (telemetria simulada) + Dockerfile
├── AgroSmart.Api.Tests/          # Testes unitários (xUnit) — cenários CT-01…CT-06
├── db/                           # Scripts SQL (DDL, seed, consultas) — tabelas AGS_*
├── docs/                         # PIPELINE.md, BUSINESS_MODEL_CANVAS.md, ER, Postman
├── docker-compose.yml            # kafka + kafka-ui + api + simulator
└── .github/workflows/            # Publicação das imagens no Docker Hub
```

---

## Segurança e autenticação

- **Login com senha criptografada (hash):** senhas são armazenadas como **hash**
  (ASP.NET Core `PasswordHasher`, PBKDF2 com salt) — nunca em texto puro.
- **Autenticação JWT:** endpoints protegidos exigem `Authorization: Bearer {token}`.
- **Validação de entrada:** DTOs com Data Annotations (`[Required]`, `[EmailAddress]`,
  `MinLength`, `MaxLength`).
- **Proteção contra SQL Injection:** acesso a dados via **EF Core** (consultas
  parametrizadas), sem concatenação de SQL.

---

## Tecnologias

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9 + **Oracle** (`Oracle.EntityFrameworkCore`)
- **Apache Kafka** (`Confluent.Kafka`) — pipeline de dados em streaming
- Autenticação **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swagger / OpenAPI** (Swashbuckle)
- **Docker** + **Docker Compose** + **Docker Hub**

---

## Modelo de dados (Oracle, prefixo `AGS_`)

| Tabela | Descrição |
|--------|-----------|
| `AGS_REGIONS` | Talhões / áreas de cultivo |
| `AGS_METRIC_TYPES` | Catálogo de métricas ambientais (unidade + faixa nominal) |
| `AGS_DEVICES` | Sensores de campo vinculados a um talhão |
| `AGS_SENSOR_READINGS` | Evento de coleta (uma leitura) |
| `AGS_MEASUREMENTS` | Cada valor de métrica de uma leitura |
| `AGS_ALERT_RULES` | Limiares configuráveis (min/max) por métrica/talhão |
| `AGS_ALERTS` | Alertas gerados (automáticos ou manuais) com ciclo de vida |
| `AGS_USERS` | Operadores autenticados via JWT |

Scripts em [`db/`](db/). Em **Development** a API aplica as **migrations do EF** na
subida (`Database.Migrate()`), cria o catálogo de métricas e **semeia talhões,
sensores e regras de alerta** de demonstração (necessários para o pipeline).

> O banco Oracle é o **mesmo da FIAP** (`oracle.fiap.com.br:1521/ORCL`, usuário
> `RM97674`). As tabelas usam prefixo `AGS_`, então coexistem com outros projetos no
> mesmo schema.

---

## Configuração do banco Oracle

1. Crie o arquivo local (não versionado) com sua senha:

```json
// AgroSmart.Api/appsettings.Development.local.json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=RM97674;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL"
  }
}
```

Em Docker, a senha vem do arquivo `.env` (veja abaixo).

---

## Como executar — Docker (recomendado: sobe todo o pipeline)

Pré-requisito: **Docker Desktop** em execução.

```bash
# 1. Crie o .env a partir do exemplo e informe ORACLE_PASSWORD (e DOCKERHUB_USERNAME)
copy .env.example .env

# 2. Suba tudo: Kafka + Kafka UI + API + Simulador
docker compose up --build -d

# 3. Acesse:
#    API/Swagger  -> http://localhost:8080/swagger
#    Kafka UI     -> http://localhost:8081   (tópico agrosmart.sensor-readings)

# 4. Acompanhe o fluxo em tempo real
docker compose logs -f simulator   # produtor publicando leituras
docker compose logs -f api         # consumer ingerindo + alertas gerados

# Encerrar
docker compose down
```

O **simulador** publica uma leitura a cada ~3s; a **API** consome do Kafka, persiste
no Oracle e dispara **alertas** quando uma medição viola uma regra.

### Publicar no Docker Hub

```bash
# login
docker login -u SEU_USUARIO

# build + tag + push (ou deixe o GitHub Actions fazer no push para main)
docker compose build
docker tag agrosmart/agrosmart-api:latest SEU_USUARIO/agrosmart-api:latest
docker tag agrosmart/agrosmart-simulator:latest SEU_USUARIO/agrosmart-simulator:latest
docker push SEU_USUARIO/agrosmart-api:latest
docker push SEU_USUARIO/agrosmart-simulator:latest
```

> Definindo `DOCKERHUB_USERNAME` no `.env`, o `docker compose build` já nomeia as
> imagens como `SEU_USUARIO/agrosmart-api` e `SEU_USUARIO/agrosmart-simulator`.
> O workflow [`.github/workflows/docker-publish.yml`](.github/workflows/docker-publish.yml)
> publica automaticamente usando os secrets `DOCKERHUB_USERNAME` e `DOCKERHUB_TOKEN`.

---

## Como executar — local (sem Docker)

```bash
# 1. Configurar a senha Oracle (appsettings.Development.local.json — ver acima)
# 2. Rodar a API (migra o schema e semeia dados de demonstração)
dotnet run --project AgroSmart.Api      # https://localhost:7118/swagger

# 3. (Opcional) rodar o simulador apontando para um Kafka local
#    Suba só o Kafka via Docker: docker compose up -d kafka
#    e habilite o consumer da API com Kafka__Enabled=true (env) ou appsettings.
dotnet run --project AgroSmart.Simulator
```

Por padrão o consumer Kafka da API fica **desabilitado** fora do Docker
(`Kafka:Enabled=false` em `appsettings.json`), para permitir `dotnet run` sem broker.

---

## Credenciais de teste (Swagger / Postman)

Em **Development**, um operador de demonstração é criado automaticamente:

| Campo | Valor |
|-------|-------|
| **E-mail** | `operador@agrosmart.com.br` |
| **Senha** | `agrosmart123` |
| **Papel** | `Operator` |

Fluxo no Swagger: **POST /api/v1/auth/login** → copie o `token` → botão **Authorize**.

```json
{ "email": "operador@agrosmart.com.br", "password": "agrosmart123" }
```

---

## Endpoints principais

| Recurso | Rota base |
|---------|-----------|
| Auth (público) | `POST /api/v1/auth/register` · `POST /api/v1/auth/login` |
| Regions (talhões) | `GET/POST/PUT/DELETE /api/v1/regions` |
| Devices (sensores) | `GET/POST/PUT/DELETE /api/v1/devices` |
| Metric Types | `GET/POST/PUT/DELETE /api/v1/metric-types` |
| Alert Rules | `GET/POST/PUT/DELETE /api/v1/alert-rules` |
| Ingestion (HTTP) | `POST /api/v1/ingestion/readings` · `/readings/batch` · `/upload` |
| Alerts | `GET /api/v1/alerts` · `PUT /{id}/status` · ... |
| Readings | `GET /api/v1/readings/{id}` · `/by-device/{deviceId}` |
| Reports | `GET /api/v1/reports/region-health` · `/alerts-summary` |
| Health | `GET /api/healthcheck` · `/api/healthcheck/full` |

> Além da ingestão por HTTP (acima), a ingestão **em tempo real** ocorre via Kafka
> (sem chamada HTTP), processada pelo `SensorReadingConsumer`.

---

## Testes unitários (xUnit)

```bash
dotnet test
```

6 cenários (CT-01…CT-06) cobrindo avaliação de limiares de alerta, unicidade de
talhão, integridade referencial e login inválido. Não exigem Oracle (usam mocks/lógica pura).
