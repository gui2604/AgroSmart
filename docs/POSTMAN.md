# AgroSmart — Guia Postman

Este guia explica como importar, configurar e usar a **collection** e o **environment** da API para testes manuais ou demonstração da entrega.

## Arquivos

| Arquivo | Descrição |
|---------|-----------|
| [`AgroSmart.postman_collection.json`](AgroSmart.postman_collection.json) | Todas as rotas agrupadas por recurso (Auth, Regions, Devices, …) |
| [`AgroSmart.postman_environment.json`](AgroSmart.postman_environment.json) | Variáveis de ambiente (`baseUrl`, `token`, etc.) |

## Pré-requisitos

1. [Postman](https://www.postman.com/downloads/) instalado (desktop ou web).
2. API em execução:
   - **Docker:** `docker compose up -d` → `http://localhost:8080`
   - **Local:** `dotnet run --project AgroSmart.Api` → `https://localhost:7118` ou `http://localhost:5268`
3. Banco Oracle FIAP acessível (connection string configurada na API).

---

## 1. Importar a collection

1. Abra o Postman.
2. Clique em **Import** (canto superior esquerdo).
3. Arraste o arquivo `AgroSmart.postman_collection.json` ou selecione **Upload Files**.
4. Confirme o nome **AgroSmart API** na lista de collections.

## 2. Importar o environment (recomendado)

1. **Import** novamente → selecione `AgroSmart.postman_environment.json`.
2. No canto superior direito, abra o seletor de environment (ícone de olho / dropdown).
3. Escolha **AgroSmart - Local**.

> Sem environment, a collection ainda funciona usando as variáveis embutidas na própria collection (`baseUrl`, `token`).

---

## 3. Configurar a URL da API

No environment **AgroSmart - Local** (ou em **Collection variables**):

| Variável | Quando usar | Valor típico |
|----------|-------------|--------------|
| `baseUrl` | API no Docker | `http://localhost:8080` |
| `baseUrlHttps` | API local com HTTPS | `https://localhost:7118` |
| `token` | Preenchido automaticamente no Login | *(deixe vazio no início)* |

**Como editar:** clique no ícone de olho → **Edit** ao lado do environment → altere `baseUrl` → **Save**.

Para `dotnet run` com HTTPS e certificado autoassinado: em **Settings** → **General**, desative **SSL certificate verification** (somente em desenvolvimento).

Todas as requisições usam `{{baseUrl}}` na URL (ex.: `{{baseUrl}}/api/v1/regions`).

---

## 4. Autenticação (obter o JWT)

A collection está configurada com **Bearer Token** herdado da collection: `Authorization: Bearer {{token}}`.

### Passo a passo

1. Abra a pasta **Auth**.
2. Execute **Login** (recomendado — usuário demo já existe em Development):

   - Método: `POST`
   - URL: `{{baseUrl}}/api/v1/auth/login`
   - Body (já preenchido na collection):

   ```json
   {
     "email": "operador@agrosmart.com.br",
     "password": "agrosmart123"
   }
   ```

3. Clique **Send**. A resposta deve ser **200 OK** com JSON contendo `"token"`.

4. O script de teste da requisição **Login** grava o token automaticamente:

   ```javascript
   var json = pm.response.json();
   if (json.token) { pm.collectionVariables.set('token', json.token); }
   ```

5. Confira: **Collection** → aba **Variables** (ou environment) → `token` deve estar preenchido.

6. Execute qualquer rota protegida (ex.: **Regions → List**) — deve retornar **200**, não **401**.

### Register (opcional)

Use **Auth → Register** apenas se quiser **outro** e-mail. Depois faça **Login** com o mesmo e-mail e senha. O body de exemplo usa as mesmas credenciais do demo; se o usuário já existir, o register retorna **400**.

### Credenciais de teste (resumo)

| Campo | Valor |
|-------|--------|
| E-mail | `operador@agrosmart.com.br` |
| Senha | `agrosmart123` |

Detalhes e troubleshooting de login: seção **Credenciais de teste** no [`README.md`](../README.md).

---

## 5. Estrutura da collection

| Pasta | Conteúdo |
|-------|----------|
| **Auth** | Register, Login (salva `token`) |
| **Regions** | CRUD de regiões |
| **Devices** | CRUD + listagem por região |
| **Metric Types** | Listar e criar métrica (catálogo seed já existe no banco) |
| **Alert Rules** | CRUD de regras de limiar |
| **Ingestion** | Leitura única, lote, upload de arquivo JSON |
| **Alerts** | Listar (filtro `status=Open`), CRUD e ciclo de vida |
| **Readings** | Consulta por id e por dispositivo |
| **Reports** | Saúde por região e resumo de alertas |
| **Health** | Liveness básico e full (Oracle) |

Rotas **Auth → Login/Register** e **Health → Basic** funcionam sem token. As demais exigem **Login** executado antes.

---

## 6. Fluxo recomendado de teste (ordem)

Siga esta ordem na primeira validação (ajuste os IDs nos bodies conforme as respostas):

| # | Pasta / requisição | O que validar |
|---|-------------------|---------------|
| 1 | **Auth → Login** | Token salvo em `{{token}}` |
| 2 | **Health → Full** | API + Oracle conectados |
| 3 | **Regions → Create** | Anote o `id` retornado |
| 4 | **Devices → Create** | Use `regionId` da etapa 3; anote `identifier` (ex. `SENSOR-A1-01`) |
| 5 | **Metric Types → List** | Anote `id` do CO2 (geralmente `3`) |
| 6 | **Alert Rules → Create** | `metricTypeId` do CO2; `regionId` opcional |
| 7 | **Ingestion → Single reading** | `deviceIdentifier` igual ao dispositivo criado |
| 8 | **Alerts → List** | Alertas gerados pela ingestão |
| 9 | **Reports → Region health (one)** | Substitua `1` pelo `regionId` real na URL |

### Ajustar IDs nas URLs e bodies

Os exemplos usam `id = 1` em paths (`/regions/1`, `/devices/1`). Após criar recursos novos, **substitua** pelos ids reais das respostas **Create** ou **List**, senão pode receber **404**.

Exemplo: se **Regions → Create** retornou `"id": 7`, use **Get by id** em `.../regions/7` e **Devices → Create** com `"regionId": 7`.

---

## 7. Ingestão de arquivo (upload)

1. Cadastre um **Device** e anote o `identifier`.
2. Edite um JSON de exemplo em `AgroSmart.Api/SampleData/` (ex. `reading_normal_A1.json`) com esse `deviceIdentifier`.
3. Abra **Ingestion → Upload file**.
4. Aba **Body** → **form-data** → campo `file` → **Select Files** → escolha o JSON.
5. **Send** → esperado **200** com `readingsProcessed` ≥ 1.

---

## 8. Collection Runner (opcional)

Para disparar várias requisições em sequência:

1. Clique com o botão direito na collection **AgroSmart API** → **Run collection**.
2. **Importante:** marque **Login** primeiro (ou desmarque rotas que precisam de ids ainda inexistentes).
3. Ajuste **Delay** se o Oracle estiver lento.
4. Verifique a aba **Test Results** — falhas **401** indicam token ausente; **404** em updates/deletes costumam ser ids de exemplo desatualizados.

Para teste automatizado completo (47 cenários), use o script PowerShell descrito no README: `scripts/test-api-endpoints.ps1`.

---

## 9. Problemas comuns

| Sintoma | Causa provável | Solução |
|---------|----------------|---------|
| **401 Unauthorized** | `token` vazio ou expirado (~4 h) | Execute **Auth → Login** novamente |
| **400 Credenciais inválidas** | Login sem usuário cadastrado | Use `operador@agrosmart.com.br` / `agrosmart123` ou **Register** antes |
| **404** em GET/PUT/DELETE | Id `1` não existe no seu banco | Use ids das respostas de **Create** / **List** |
| **400** dispositivo duplicado | `identifier` já usado | Troque o `identifier` no **Devices → Create** |
| **404** na ingestão | `deviceIdentifier` não cadastrado | Crie o device antes; alinhe o JSON |
| Erro de SSL | HTTPS local com certificado dev | Desative verificação SSL no Postman (dev only) |
| Connection refused | API parada ou `baseUrl` errada | Suba a API; confira `baseUrl` (8080 vs 7118) |

---

## 10. Variáveis: collection vs environment

| Escopo | Onde editar | Prioridade |
|--------|-------------|------------|
| **Environment** | AgroSmart - Local | Recomendado para `baseUrl` por máquina |
| **Collection** | AgroSmart API → Variables | `token` é gravado aqui pelo script do Login |

Se `token` não atualizar após Login, verifique se o environment selecionado não sobrescreve com valor vazio — use **Current value** na collection ou no environment de forma consistente.
