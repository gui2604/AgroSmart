# Business Model Canvas — AgroSmart

Modelo de negócio da plataforma **AgroSmart**: monitoramento ambiental e agricultura
de precisão para o campo (lavouras, pastagens e estufas), transformando dados de
sensores em decisões agronômicas e redução de perdas.

> Requisito 3 da Fase 4. Abaixo o Canvas em formato de tabela e, na sequência,
> o detalhamento de cada bloco.

| **Parcerias-chave** | **Atividades-chave** | **Proposta de valor** | **Relacionamento** | **Segmentos de clientes** |
|---|---|---|---|---|
| Fabricantes de sensores/IoT (LoRa, NB-IoT)<br>Cooperativas agrícolas<br>Revendas de insumos<br>Provedores de nuvem<br>Universidades/Embrapa<br>Operadoras de conectividade rural | Desenvolvimento da plataforma e pipeline de dados em tempo real<br>Curadoria de regras agronômicas/alertas<br>Modelos de classificação/visão computacional<br>Suporte e onboarding<br>Segurança e LGPD | **Decisões agronômicas em tempo real**: alertas automáticos de risco (estresse hídrico, calor, pH do solo) que reduzem perdas e custos de insumos, aumentam produtividade e dão rastreabilidade — acessível via API e dashboards, com baixo custo de implantação. | Self-service (SaaS) + Swagger/API<br>Suporte técnico e agronômico<br>Sucesso do cliente nas cooperativas<br>Comunidade e treinamentos | **Médios e grandes produtores** (grãos, hortaliças, café)<br>**Cooperativas** e agroindústrias<br>**Agrônomos/consultores**<br>**Startups de AgTech** (via API)<br>**Produtores de estufa** |
| | **Recursos-chave** | | **Canais** | |
| | Plataforma (.NET 9 + Kafka + Oracle)<br>Pipeline de streaming<br>Base de dados de leituras/alertas<br>Equipe de eng. + agronomia<br>Marca e parcerias | | Venda direta (B2B)<br>Cooperativas e revendas (canal)<br>Marketplace de nuvem<br>API/integradores<br>Eventos e demonstrações no campo | |
| **Estrutura de custos** || | **Fontes de receita** ||
| Infraestrutura de nuvem e streaming (Kafka)<br>Licença/banco Oracle<br>P&D e folha (eng./agronomia)<br>Suporte e sucesso do cliente<br>Comercial/marketing<br>Hardware de sensores (quando ofertado) ||| Assinatura SaaS por hectare/sensor (mensal)<br>Setup/onboarding<br>Tiers por volume de dados e features (alertas, IA)<br>Venda/locação de kits de sensores<br>API/uso para integradores (AgTechs)<br>Relatórios e consultoria premium ||

---

## Detalhamento dos blocos

### 1. Segmentos de clientes
- **Médios e grandes produtores** de grãos (soja, milho), hortaliças e café que buscam
  produtividade e redução de perdas.
- **Cooperativas e agroindústrias** que precisam de visão consolidada de várias fazendas.
- **Agrônomos e consultores** que usam os dados/alertas para recomendações.
- **Estufas e fazendas verticais** com necessidade de controle climático fino.
- **Startups de AgTech** que consomem a **API** para compor seus próprios produtos.

### 2. Proposta de valor
- **Monitoramento em tempo real** de variáveis ambientais por talhão/estufa.
- **Alertas automáticos** (regras configuráveis de mín./máx. por métrica e região):
  estresse hídrico, calor, pH/condutividade do solo fora de faixa.
- **Redução de perdas e de uso de insumos** (água, fertilizante, energia).
- **Rastreabilidade e relatórios** de salubridade por região.
- **Baixo custo de adoção**: API aberta, containerizada, integrável.
- **Escalável**: pipeline de streaming (Kafka) suporta milhares de sensores.

### 3. Canais
- Venda direta B2B e via **cooperativas/revendas** (capilaridade no campo).
- **Marketplaces de nuvem** e **API** para integradores/AgTechs.
- Demonstrações no campo, eventos do agro e conteúdo técnico.

### 4. Relacionamento com clientes
- **SaaS self-service** com documentação (Swagger) e onboarding assistido.
- **Suporte agronômico e técnico**; equipe de **sucesso do cliente** em cooperativas.
- Comunidade, treinamentos e base de conhecimento.

### 5. Fontes de receita
- **Assinatura recorrente (SaaS)** por hectare e/ou por sensor.
- **Tiers de planos** por volume de dados e recursos (alertas avançados, IA/visão).
- **Setup/onboarding** e **consultoria/relatórios premium**.
- **Venda ou locação de kits de sensores**.
- **Uso de API** para integradores (modelo de plataforma).

### 6. Recursos-chave
- Plataforma **AgroSmart** (.NET 9, ASP.NET Core, EF Core/Oracle).
- **Pipeline de dados em streaming** (Apache Kafka) e base histórica de leituras/alertas.
- Equipe de **engenharia + agronomia**; regras agronômicas e modelos de classificação.
- Marca, parcerias e relacionamento com cooperativas.

### 7. Atividades-chave
- Operar e evoluir o **pipeline em tempo real** e a API.
- Curadoria de **regras de alerta** e modelos de **classificação/visão**.
- **Segurança da informação** e conformidade **LGPD**.
- Onboarding, suporte e expansão de contas.

### 8. Parcerias-chave
- **Fabricantes de sensores/IoT** e conectividade rural (LoRaWAN, NB-IoT, satélite).
- **Cooperativas, revendas e agroindústrias** (canal e dados).
- **Provedores de nuvem** e **instituições de pesquisa** (Embrapa, universidades).

### 9. Estrutura de custos
- **Infraestrutura** de nuvem e streaming (Kafka), **licença Oracle**.
- **P&D e folha** (engenharia e agronomia).
- **Suporte/sucesso do cliente**, **comercial e marketing**.
- **Hardware de sensores** (quando incluído na oferta).

---

## Indicadores de viabilidade (resumo)
- **Modelo recorrente (SaaS)** com baixa rotatividade quando integrado à operação da fazenda.
- **Custo marginal baixo** por novo sensor/cliente (plataforma multi-tenant + streaming).
- **Escalabilidade técnica** comprovada pelo pipeline Kafka + containerização.
- **Ganho mensurável** para o produtor (redução de perdas e de insumos) sustenta o preço por hectare.
