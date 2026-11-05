# Ponte Games API - CitizenTech & Logística Reversa

API RESTful desenvolvida em **.NET 8** concebida para a iniciativa **Ponte Games**, uma solução de *CitizenTech* focada na gamificação da recolha seletiva, economia circular e incentivo ao descarte consciente de resíduos urbanos (alinhada aos Objetivos de Desenvolvimento Sustentável da ONU 11 - Cidades Sustentáveis e 12 - Consumo e Produção Responsáveis).

---

## Integrantes do Grupo

- **RM 562673** - Geovanne Coneglian Passos - Turma: 2TDSPX
- **RM 563960** - Lucas Silva Gastão Pinheiro - Turma: 2TDSPX
- **RM 563143** - Guilherme Soares De Almeida - Turma: 2TDSPX

---

## Arquitetura e Tecnologias Utilizadas

A solução adota padrões de **Arquitetura Limpa (Clean Architecture)** e **Minimal APIs**, assegurando baixa latência, código modular e separação estrita de responsabilidades:

- **Plataforma:** .NET 8 (C# 12) com ASP.NET Core Minimal APIs
- **Persistência de Dados:** Entity Framework Core (SQLite local configurável para Oracle Database)
- **Documentação Interativa:** Swagger / OpenAPI 2.0 (Swashbuckle com suporte a JWT e esquemas tipados)
- **Segurança:** Autenticação e Autorização baseada em tokens JWT (JSON Web Tokens)
- **Validação de Entrada:** FluentValidation com limites operacionais anti-fraude por tipo de material
- **Concorrência e Idempotência:** Filtro de endpoint customizado via cabeçalho `X-Idempotency-Key` (HTTP 409 Conflict)
- **Resiliência:** Rate Limiting nativo em janela fixa com tratamento estruturado (HTTP 429 Too Many Requests)
- **Monitoramento:** Health Checks nativos em `/health` avaliando a integridade do serviço e da base de dados
- **Qualidade de Software:** Suite automatizada de testes com xUnit, FluentAssertions e `WebApplicationFactory`

---

## Estrutura do Projeto

```text
PonteGames/
├── src/
│   └── PonteGames.Api/
│       ├── Common/             # Filtros de Idempotência e Tratamento Global
│       ├── Data/               # DbContext e Mapeamento de Entidades
│       ├── DTOs/               # Contratos de Transferência de Dados
│       ├── Endpoints/          # Mapeamento Modular de Rotas Minimal API
│       ├── Models/             # Entidades de Domínio (Usuario, Descarte)
│       ├── Services/           # Regras de Negócio e Cálculo de Gamificação (XP)
│       ├── Validators/         # Regras do FluentValidation por Categoria
│       ├── appsettings.json    # Parâmetros e Configuração de JWT
│       └── Program.cs          # Pipeline HTTP e Injeção de Dependências
└── tests/
    └── PonteGames.Tests/       # Testes Unitários e Testes de Integração

```



## Regras de Negócio e Gamificação

# Cálculo de XP por Categoria:

Eletrónico: 50 XP por kg descartado.

Óleo de Cozinha: 40 XP por litro descartado.

Metal: 20 XP por kg descartado.

Plástico: 15 XP por kg descartado.

Papel: 10 XP por kg descartado.

# Limites de Descarte Residencial (Prevenção de Abusos):

Óleo: Limite individual de 5 L/kg.

Eletrónico: Limite individual de 20 kg.

Plástico / Metal / Papel: Limite individual de 10 kg.

Tentativas de descarte acima destes parâmetros são rejeitadas com 400 Bad Request padronizado via RFC 7807.

Proteção contra Duplicidade (Idempotência):

Requisições para o endpoint POST /api/descartes exigem o cabeçalho X-Idempotency-Key.

Chaves já processadas são barradas com código 409 Conflict, impedindo computação duplicada de XP.

## Como Executar a Aplicação Localmente

Pré-requisitos

.NET 8 SDK instalado

Visual Studio 2022 ou VS Code

1. Clonar o Repositório

2. Executar a API

Abra o terminal na pasta da API e inicie o serviço:

cd PonteGames.Api

dotnet run

3. Aceder à Documentação Swagger

Abra o navegador no caminho configurado:

http://localhost:5023/swagger

## Guia de Teste e Demonstração no Swagger

Autenticação:

Execute o endpoint POST /api/auth/login informando {"email": "geovanne@pontegames.com"}.

Copie o token retornado na resposta.

Clique no botão verde Authorize no topo da página e introduza: Bearer <SEU_TOKEN>.

Registo de Descarte Sustentável:

Aceda a POST /api/descartes.

Defina o cabeçalho X-Idempotency-Key (exemplo: teste-001).

Forneça o corpo da requisição:

{
  "usuarioId": 1,
  "categoriaResiduo": "eletronico",
  "pesoKg": 2.0
}

O resultado será 201 Created e atribuirá 100 XP ao cidadão.

Reenvie a mesma requisição sem alterar a chave para comprovar o bloqueio 409 Conflict.

Consulta de Ranking e Histórico:

Aceda a GET /api/ranking para visualizar os líderes comunitários atualizados.

Aceda a GET /api/descartes/historico/1 para verificar o histórico do cidadão.

## Monitoramento e Integridade:

Aceda a http://localhost:5023/health para obter o status de saúde da aplicação e da base de dados.

## Execução dos Testes Automatizados

Para executar os testes unitários e de integração via linha de comandos:

dotnet test

Declaração de Uso de Ferramentas de IA

Conforme estabelecido pelas diretrizes académicas da Global Solution

declara-se a utilização de ferramentas de Inteligência Artificial generativa (Gemini / Copilot) como suporte no auxílio à escrita e refatoração de código

configurações de filtros de middleware e redação desta documentação técnica. 

Toda a validação arquitetural, testes de conformidade e depuração foram conduzidos e validados pela equipa de desenvolvimento.