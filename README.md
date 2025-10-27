# API Mottu – Gestão de Pátios, Funcionários e Gerentes

> **3ª Sprint – ADVANCED BUSINESS DEVELOPMENT WITH .NET**  
> Projeto em ASP.NET Core Web API seguindo boas práticas REST (paginação, HATEOAS, status codes adequados) e documentação via Swagger/OpenAPI.

---

## 👥 Integrantes da Equipe
- Nome 1 — RM: 558935 — Lu Vieira Santos
- Nome 2 — RM: 555656 — Melissa Pereira
- Nome 3 — RM: 558755 — E‑mail: Diego Furigo

---

## 🧭 Domínio e Justificativa
O domínio escolhido representa a **operação de pátios da Mottu**. Há relação natural entre **pátios**, **funcionários** e a **designação de gerentes** por pátio — cenário comum em operações logísticas reais. Isso permite avaliar relacionamentos 1‑N e N‑N simples, além de autenticação e gestão de acesso futura.

### Entidades principais (mínimo 3)
1. **Pátio** (`Patio`) – local físico onde motos/equipes operam.  
2. **Funcionário** (`Funcionario`) – usuário operacional, alocado em um pátio.  
3. **Gerente** (`Gerente`) – designa qual funcionário gerencia um determinado pátio (relação Funcionario ↔ Pátio).

> Essas 3 entidades cobrem o requisito de **“mínimo 3 entidades principais”** e fazem sentido de negócio, pois a operação diária depende de cadastro de pátios, quadro de pessoas e responsáveis por cada pátio.

---

## 🏗️ Arquitetura e Tecnologias
- **ASP.NET Core Web API** (.NET 9)
- **Entity Framework Core** (mapeamento ORM)
- **Banco**: *Oracle* (produção/aula) e opção de **banco em memória** no ambiente de desenvolvimento para facilitar testes locais (ver *Execução*).
- **Swagger/OpenAPI** para documentação com autenticação JWT
- **Paginação** via `page` e `pageSize`
- **HATEOAS**: respostas incluem links de navegação (coleções e recursos) para facilitar descoberta de rotas
- **Versionamento de API** via URL (v1, v2...)
- **Health Check** para monitoramento
- **Autenticação JWT** (JSON Web Tokens) completa
- **ML.NET** para previsões de demanda de funcionários
- **xUnit** para testes unitários e de integração
- **Camadas do projeto**
  - `Controllers/` – entrada HTTP e contratos REST
  - `Data/` – `DbContext`, mapeamentos e migrações
  - `Models/` – entidades de domínio e DTOs
  - `Services/` – lógica de negócio e ML.NET
  - `Migrations/` – versionamento de esquema (EF Core)

> Justificativa: A separação por camadas simplifica manutenção e testes, enquanto EF Core acelera o desenvolvimento seguro com Oracle. Swagger garante **transparência dos contratos** e acelera QA. JWT garante autenticação segura e ML.NET permite previsões inteligentes.

---

## ▶️ Execução (Dev e Produção)

### Pré‑requisitos
- **Visual Studio 2022** (17.12+) ou **.NET 9 SDK** instalado
- Acesso ao Oracle (se for usar DB real) ou executar em **modo Dev** com banco em memória

### 1) Clonar e restaurar
```bash
git clone <url-do-repo>
cd sprint3-.Net/MottuApi
dotnet restore
```

### 2) Definir ambiente e banco
**Modo rápido (DEV – sem Oracle):**
- O projeto está configurado para rodar com banco **InMemory** quando `ASPNETCORE_ENVIRONMENT=Development`.  
- Nesse modo você já consegue abrir o Swagger e exercitar os endpoints.

**Modo com Oracle (produção/aula):**
1. Ajuste a connection string `DefaultConnection` no `appsettings.json`.
2. Aplique as migrações:
   ```bash
   dotnet tool install --global dotnet-ef   # se ainda não tiver
   dotnet ef database update
   ```

### 3) Rodar
```bash
dotnet run
```
- Logs mostram algo como: `Now listening on: http://localhost:5008`
- **Swagger UI**: por padrão está na **raiz** → **http://localhost:5008/**
  - Se preferir em `/swagger`, altere `Program.cs` para `c.RoutePrefix = "swagger";` e acesse `http://localhost:5008/swagger`.
- HTTPS opcional: habilite no `launchSettings.json` (`applicationUrl` com http **e** https).

---

## 📘 Swagger / OpenAPI
- **Descrição de endpoints** e **parâmetros** com anotações
- **Exemplos de payload** incluídos
- **Modelos de dados** descritos
- Acesse a documentação interativa em **http://localhost:5008/** (ou `/swagger` se configurado)

---

## 🔗 Endpoints (CRUD + Paginação + HATEOAS + Autenticação + ML.NET)

> **Versionamento**: todos os endpoints usam `/api/v1/` como prefixo.
> **Paginação**: use `?page=1&pageSize=10`.
> **HATEOAS**: as respostas incluem `links` com `rel`, `href` e `method` (exemplos abaixo).
> **Status codes**: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `401 Unauthorized`, `404 Not Found`, `409 Conflict` (quando aplicável).
> **Autenticação**: Sistema JWT completo com tokens seguros.

### Health Check
- `GET /health` – Verifica saúde da API

**Exemplo**
```bash
curl -X GET "http://localhost:5008/health"
```
**Resposta 200**
```
Healthy
```

### Autenticação
- `POST /api/v1/auth/login` – Login de funcionário com JWT

**Exemplo – login**
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "funcionario1@mottu.com",
  "senha": "senha123"
}
```

**Resposta 200**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "funcionario": {
    "id": 1,
    "nome": "Funcionário 1",
    "email": "funcionario1@mottu.com",
    "patioId": 1,
    "patio": {
      "id": 1,
      "nome": "Pátio Central",
      "endereco": "Rua das Flores, 123 - Centro"
    }
  }
}
```

### Previsão (ML.NET)
- `POST /api/v1/previsao/ocupacao-patio` – Prevê ocupação de funcionários
- `GET /api/v1/previsao/info` – Informações sobre o modelo ML

**Exemplo – previsão**
```http
POST /api/v1/previsao/ocupacao-patio
Content-Type: application/json

{
  "diaDaSemana": 1,
  "hora": 12,
  "mesDoAno": 1
}
```

**Resposta 200**
```json
{
  "data": {
    "numeroFuncionariosPrevisto": 42,
    "periodo": "Tarde",
    "recomendacao": "Alto movimento previsto para Segunda-feira. Recomenda-se escala completa de funcionários."
  },
  "links": {
    "self": "/api/v1/previsao/ocupacao-patio",
    "documentation": "/swagger"
  }
}
```

### Pátios
- `GET /api/v1/patios` – lista (paginação)
- `GET /api/v1/patios/{id}` – detalhe
- `POST /api/v1/patios` – cria
- `PUT /api/v1/patios/{id}` – atualiza
- `DELETE /api/v1/patios/{id}` – exclui

**Exemplo – criação**
```http
POST /api/v1/patios
Content-Type: application/json

{
  "nome": "Pátio Central",
  "endereco": "Rua Principal, 123"
}
```
**Resposta 201**
```json
{
  "id": 1,
  "nome": "Pátio Central",
  "endereco": "Rua Principal, 123",
  "links": [
    {"rel":"self","href":"/api/v1/patios/1","method":"GET"},
    {"rel":"update","href":"/api/v1/patios/1","method":"PUT"},
    {"rel":"delete","href":"/api/v1/patios/1","method":"DELETE"}
  ]
}
```

### Funcionários
- `GET /api/v1/funcionarios` – lista (paginação, sem exposição de senhas)
- `GET /api/v1/funcionarios/{id}` – detalhe (sem exposição de senha)
- `POST /api/v1/funcionarios` – cria (com hash automático de senha)
- `PUT /api/v1/funcionarios/{id}` – atualiza
- `DELETE /api/v1/funcionarios/{id}` – exclui

**Exemplo – criação**
```http
POST /api/v1/funcionarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@mottu.com",
  "senha": "123456",
  "patioId": 1
}
```

**Resposta 201** (senha não é retornada por segurança)
```json
{
  "data": {
    "id": 1,
    "nome": "João Silva",
    "email": "joao@mottu.com",
    "patioId": 1,
    "patio": {
      "id": 1,
      "nome": "Pátio Central",
      "endereco": "Rua das Flores, 123 - Centro"
    }
  },
  "links": {
    "self": "/api/v1/funcionarios/1",
    "update": "/api/v1/funcionarios/1",
    "delete": "/api/v1/funcionarios/1"
  }
}
```

### Gerentes
- `GET /api/v1/gerentes`
- `GET /api/v1/gerentes/{id}`
- `POST /api/v1/gerentes`
- `PUT /api/v1/gerentes/{id}`
- `DELETE /api/v1/gerentes/{id}`

**Exemplo – criação**
```http
POST /api/v1/gerentes
Content-Type: application/json

{
  "funcionarioId": 1,
  "patioId": 1
}
```

---

## 🛠️ Exemplos de Uso com cURL

### Autenticação
```bash
# Login
curl -X POST "http://localhost:5008/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "joao.silva@mottu.com",
       "senha": "123456"
     }'
```

### Pátios
```bash
# Listar pátios (paginado)
curl -X GET "http://localhost:5008/api/patios?page=1&pageSize=10"

# Criar pátio
curl -X POST "http://localhost:5008/api/patios" \
     -H "Content-Type: application/json" \
     -d '{
       "nome": "Pátio Central",
       "endereco": "Rua Principal, 123"
     }'

# Obter pátio por ID
curl -X GET "http://localhost:5008/api/patios/1"

# Atualizar pátio
curl -X PUT "http://localhost:5008/api/patios/1" \
     -H "Content-Type: application/json" \
     -d '{
       "id": 1,
       "nome": "Pátio Central Atualizado",
       "endereco": "Rua Principal, 123"
     }'

# Excluir pátio
curl -X DELETE "http://localhost:5008/api/patios/1"
```

### Funcionários
```bash
# Listar funcionários (paginado)
curl -X GET "http://localhost:5008/api/funcionarios?page=1&pageSize=10"

# Criar funcionário
curl -X POST "http://localhost:5008/api/funcionarios" \
     -H "Content-Type: application/json" \
     -d '{
       "nome": "João Silva",
       "email": "joao@mottu.com",
       "senha": "123456",
       "patioId": 1
     }'

# Obter funcionário por ID
curl -X GET "http://localhost:5008/api/funcionarios/1"

# Atualizar funcionário
curl -X PUT "http://localhost:5008/api/funcionarios/1" \
     -H "Content-Type: application/json" \
     -d '{
       "id": 1,
       "nome": "João Silva Atualizado",
       "email": "joao@mottu.com",
       "senha": "123456",
       "patioId": 1
     }'

# Excluir funcionário
curl -X DELETE "http://localhost:5008/api/funcionarios/1"
```

### Gerentes
```bash
# Listar gerentes (paginado)
curl -X GET "http://localhost:5008/api/gerentes?page=1&pageSize=10"

# Criar gerente
curl -X POST "http://localhost:5008/api/gerentes" \
     -H "Content-Type: application/json" \
     -d '{
       "funcionarioId": 1,
       "patioId": 1
     }'

# Obter gerente por ID
curl -X GET "http://localhost:5008/api/gerentes/1"

# Atualizar gerente
curl -X PUT "http://localhost:5008/api/gerentes/1" \
     -H "Content-Type: application/json" \
     -d '{
       "id": 1,
       "funcionarioId": 1,
       "patioId": 1
     }'

# Excluir gerente
curl -X DELETE "http://localhost:5008/api/gerentes/1"
```

---

## 🧪 Testes

Este projeto inclui uma suíte completa de testes unitários e de integração usando **xUnit**.

### Testes Implementados

#### Testes Unitários
- **AuthServiceTests**: Testes do serviço de autenticação
  - Login com credenciais válidas
  - Login com email inválido
  - Login com senha inválida
  - Hash de senhas (consistência e unicidade)

- **PatioPrevisaoServiceTests**: Testes do serviço ML.NET
  - Previsão com dados válidos
  - Previsão em diferentes horários e dias
  - Identificação correta de períodos
  - Validação de movimento em dias de semana vs fim de semana
  - Geração de recomendações

- **UnitTest1**: Testes básicos de modelos
  - Criação de Pátio com propriedades válidas
  - Criação de Funcionário com propriedades válidas
  - Criação de Gerente com propriedades válidas

#### Testes de Integração
- **ApiIntegrationTests**: Testes end-to-end com WebApplicationFactory
  - Health check endpoint
  - Listar pátios, funcionários e gerentes
  - Previsão ML.NET com dados válidos e inválidos
  - Login com credenciais válidas e inválidas
  - Disponibilidade do Swagger

### Executar Testes

Para executar todos os testes:
```bash
# Navegar até a pasta raiz do projeto
cd Sprint3-Mottu-.Net-API

# Executar todos os testes
dotnet test

# Executar com saída detalhada
dotnet test --logger "console;verbosity=detailed"

# Executar com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### Estrutura de Testes
```
MottuApi.Tests/
├── Services/
│   ├── AuthServiceTests.cs
│   └── PatioPrevisaoServiceTests.cs
├── Integration/
│   └── ApiIntegrationTests.cs
└── UnitTest1.cs
```

### Resultados Esperados
Todos os testes devem passar:
- ✅ AuthServiceTests: 5 testes
- ✅ PatioPrevisaoServiceTests: 8 testes
- ✅ UnitTest1: 3 testes
- ✅ ApiIntegrationTests: 10 testes

**Total: 26 testes aprovados**

---

## 🚀 Melhorias Implementadas

### Segurança (25 pontos)
- ✅ **JWT Authentication** completo com tokens seguros
- ✅ **Hash de senhas** com SHA256 antes de salvar no banco
- ✅ **Configuração JWT** no Swagger para teste de endpoints protegidos
- ✅ **DTOs de resposta** que não expõem senhas
- ✅ **Validação de email único** na criação de funcionários
- ✅ **Validação de pátio existente** na criação de funcionários

### Versionamento (10 pontos)
- ✅ **API Versioning** via URL (v1)
- ✅ **Configuração Asp.Versioning** para suporte a múltiplas versões
- ✅ **Todos os controllers versionados** com atributo ApiVersion

### Health Check (10 pontos)
- ✅ **Endpoint /health** implementado
- ✅ **Health Checks** configurado no pipeline

### Machine Learning (25 pontos)
- ✅ **ML.NET** integrado ao projeto
- ✅ **Modelo de previsão** de ocupação de pátios
- ✅ **Endpoint POST /api/v1/previsao/ocupacao-patio** funcional
- ✅ **Algoritmo SDCA** para regressão
- ✅ **Recomendações inteligentes** baseadas nas previsões

### Testes (30 pontos)
- ✅ **xUnit** como framework de testes
- ✅ **26 testes unitários** cobrindo lógica principal
- ✅ **Testes de integração** com WebApplicationFactory
- ✅ **Testes de AuthService** (login, hash, validações)
- ✅ **Testes de PatioPrevisaoService** (ML.NET)
- ✅ **Testes de API endpoints** (health, CRUD, ML)
- ✅ **Moq** para mocking de dependências
- ✅ **InMemory Database** para testes isolados

### Documentação Swagger
- ✅ **Swagger UI** completo e atualizado
- ✅ **Descrições detalhadas** de endpoints
- ✅ **Esquemas de autenticação** JWT documentados
- ✅ **Exemplos de requisição/resposta**

### Dados de Exemplo
- ✅ **Seed data** automático em desenvolvimento
- ✅ **Dados pré-cadastrados** para testes
- ✅ **Funcionários de exemplo** com credenciais conhecidas

### Qualidade do Código
- ✅ **Separação de responsabilidades** com serviços
- ✅ **DTOs específicos** para criação e resposta
- ✅ **Validações robustas** nos endpoints
- ✅ **Tratamento de erros** adequado
- ✅ **Código limpo** e bem estruturado

## ✅ Checklist vs. Requisitos

### Requisitos Obrigatórios
- [x] **10 pts** – Health check endpoint (/health) ✅
- [x] **10 pts** – Versionamento da API (v1) ✅
- [x] **25 pts** – Segurança JWT completa ✅
- [x] **25 pts** – Endpoint usando ML.NET ✅
- [x] **30 pts** – Testes unitários com xUnit ✅
- [x] **Swagger** – Documentação atualizada ✅
- [x] **WebApplicationFactory** – Testes de integração ✅
- [x] **README** – Instruções para executar testes ✅

### Pontuação Total: 100 pontos ✅

### Boas Práticas REST
- [x] **3 entidades** principais (Pátio, Funcionário, Gerente) com justificativa
- [x] **CRUD** completo para as 3 entidades
- [x] **Verbos HTTP** corretos (GET, POST, PUT, DELETE)
- [x] **Status codes** adequados (200, 201, 204, 400, 401, 404)
- [x] **Paginação** (page, pageSize) em coleções
- [x] **HATEOAS** para navegação entre recursos
- [x] **Validações** de dados de entrada

### Prevenção de Penalidades
- [x] ✅ **Swagger atualizado** (evita -20 pts)
- [x] ✅ **Projeto compila** (evita -100 pts)
- [x] ✅ **README atualizado** (evita -20 pts)

---

## 📄 Licença
Uso acadêmico. Ajuste conforme a política da disciplina.

