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

## 💾 Persistência de Dados: Por que SQLite?

A aplicação utiliza o **SQLite** como mecanismo de banco de dados relacional via **Entity Framework Core** (`Microsoft.EntityFrameworkCore.Sqlite`).

### Motivações da Escolha Técnica:
1. **Zero External Dependencies / Portabilidade Total:** O banco roda em um arquivo local em disco (`pontegames.db`), eliminando a necessidade de instalar instâncias locais de Oracle, SQL Server ou Docker para rodar e avaliar a API.
2. **Segurança e Zero Vazamento de Credenciais:** Elimina riscos de versionar senhas, IPs ou strings corporativas no GitHub.
3. **Seed Automático em Execução:** Ao iniciar a aplicação, o método `db.Database.EnsureCreated()` cria automaticamente as tabelas e alimenta os usuários iniciais caso a base esteja vazia.
4. **Facilidade de Avaliação pela Banca:** O professor ou avaliador só precisa executar `dotnet run` e o banco estará 100% operacional imediatamente.

---

## 🏗️ Arquitetura Detalhada: O que faz cada item no código?

A aplicação foi estruturada segundo as melhores práticas de **Clean Architecture** e princípios de **Baixo Acoplamento e Alta Coesão**, dividindo as responsabilidades em diretórios especializados:

### 1. `Program.cs` (Pipeline HTTP e IoC Container)
- **Função:** Ponto de entrada da aplicação. Configura o contêiner de injeção de dependências, registra o `DbContext`, o `GamificationService`, os validadores do FluentValidation, autenticação JWT, políticas de Rate Limiting (HTTP 429), Swagger OpenAPI 2.0 e Health Checks. Conecta os grupos de rotas via métodos de extensão.

---

### 2. Pasta `Models/` (Entidades de Domínio)
Representam as tabelas e o modelo relacional do banco de dados:

- **`Usuario.cs`**:
  - Armazena a identidade do cidadão (`Id`, `Nome`, `Email`, `Role`).
  - Guarda a evolução do usuário na plataforma: `PontosXp` (experiência acumulada) e `Nivel` (calculado dinamicamente).
- **`Descarte.cs`**:
  - Representa o evento físico de entrega do resíduo em um ecoponto.
  - Guarda a referência do usuário (`UsuarioId`), a categoria (`CategoriaResiduo`), o total de XP gerado (`XpGanho`), a data (`DataRegistro`) e a chave única de transação (`IdempotencyKey`).

---

### 3. Pasta `DTOs/` (Contratos de Transferência de Dados)
Isolam o modelo do banco de dados das requisições externas para segurança e desacoplamento:

- **`AuthDtos.cs`**:
  - `LoginRequestDto`: Recebe apenas o e-mail do cidadão para identificação e emissão de credenciais.
  - `LoginResponseDto`: Devolve o token JWT gerado e o resumo do cidadão.
  - `UsuarioDto`: Projeta os dados seguros do usuário (ocultando campos sensíveis ou internos).
- **`DescarteDtos.cs`**:
  - `RegistroDescarteDto`: Payload de entrada do descarte contendo `UsuarioId`, `CategoriaResiduo` e `PesoKg`.
  - `DescarteResponseDto`: Contrato de retorno após registro bem-sucedido (`Id`, `XpGanho`, data).
  - `RankingItemDto`: Contrato otimizado para visualização da tabela dos melhores cidadãos recicladores.

---

### 4. Pasta `Endpoints/` (Rotas e Handlers HTTP)
Organizam as rotas em módulos estáticos usando Minimal APIs:

- **`AuthEndpoints.cs`**:
  - Mapeia `POST /api/auth/login`. Localiza o usuário cadastrado, gera o token JWT assinado criptograficamente com chave HMAC-SHA256 e Claims (`NameIdentifier`, `Name`, `Role`).
- **`DescarteEndpoints.cs`**:
  - Mapeia `POST /api/descartes`: Protegido por JWT, Rate Limiting e filtro de idempotência. Dispara o validador, calcula o XP e salva o registro no banco.
  - Mapeia `GET /api/descartes/historico/{usuarioId}`: Retorna a linha do tempo de descartes do usuário ordenados por data decrescente.
- **`RankingEndpoints.cs`**:
  - Mapeia `GET /api/ranking`: Consulta os 10 cidadãos com maior pontuação, ordenando por XP descendente.

---

### 5. Pasta `Services/` (Lógica de Negócios e Gamificação)
- **`IGamificationService.cs` & `GamificationService.cs`**:
  - Isola a matemática das regras de gamificação da camada de transporte HTTP.
  - `CalcularXp(string categoria, double pesoKg)`: Aplica os multiplicadores oficiais por categoria (Eletrônico = 50, Óleo = 40, Metal = 20, Plástico = 15, Papel = 10).
  - `CalcularNivel(int xpTotal)`: Converte o XP em faixas de nível para engajamento cívico.

---

### 6. Pasta `Validators/` (Defesa e Regras de Negócio Anti-Fraude)
- **`RegistroDescarteValidator.cs`**:
  - Implementado com **FluentValidation**.
  - **Prevenção de Abusos e Fraudes:** Impede registros com peso nulo/negativo e barra volumes incompatíveis com o perfil residencial do cidadão (máximo de 5L para óleo de cozinha, 20kg para eletrônicos e 10kg para recicláveis comuns).
  - Retorna `400 Bad Request` padronizado sob a RFC 7807 (`ValidationProblem`).

---

### 7. Pasta `Common/` (Resiliência e Idempotência)
- **`IdempotencyFilter.cs`**:
  - Filtro nativo de Minimal API (`IEndpointFilter`).
  - Lê o cabeçalho HTTP `X-Idempotency-Key`.
  - Impede processamento duplo: se a mesma chave for enviada consecutivamente (por instabilidade de rede ou duplo clique), aborta a operação retornando `409 Conflict`.

---

## 🧪 Suíte de Testes Automatizados: Por que cada teste existe?

No projeto **`PonteGames.Tests`**, combinamos **xUnit**, **FluentAssertions** e **`WebApplicationFactory`** para cobrir cenários críticos de ponta a ponta:

| Teste | Tipo | Justificativa e Cenário Validado |
| :--- | :--- | :--- |
| `CalcularXp_ComCategoriasValidas_DeveCalcularCorretamente` | **Unitário** | Garante que a fórmula de pontuação de XP por peso/material não sofre regressões aritméticas. |
| `CalcularNivel_DeveEvoluirConformeXp` | **Unitário** | Comprova a lógica de progressão de nível do cidadão na plataforma de gamificação. |
| `GetHealth_DeveRetornarStatus200Ok` | **Integração** | Valida a rota `/health` garantindo que a aplicação e o SQLite estão vivos e operacionais. |
| `GetRanking_DeveRetornarListaDeUsuariosComSucesso` | **Integração** | Testa a serialização JSON e a listagem ordenada de classificação comunitária. |
| `PostDescarte_SemAutenticacao_DeveRetornarUnauthorized` | **Integração** | Garante que requisições sem o Token JWT no cabeçalho Authorization são barradas com `401 Unauthorized`. |
| `PostDescarte_SemCabecalhoIdempotencyKey_DeveRetornarBadRequest` | **Integração** | Garante a obrigatoriedade da chave de rastreabilidade para prevenção de inconsistências no banco. |
| `PostDescarte_ComMesmaIdempotencyKeyDuasVezes_DeveRetornarConflict` | **Integração** | Simula envio concorrente idêntico e valida se a 2ª chamada é rejeitada com `409 Conflict`. |

---


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

### 🤖 Declaração de Uso de Ferramentas de IA
Conforme as diretrizes da Global Solution, declara-se o uso de ferramentas de Inteligência Artificial generativa (Gemini / GitHub Copilot) exclusivamente para apoio à estruturação de classes, refatoração de código e elaboração da documentação técnica. Toda a validação da arquitetura, depuração das rotas e execução dos testes foram realizadas pelos integrantes do grupo.
