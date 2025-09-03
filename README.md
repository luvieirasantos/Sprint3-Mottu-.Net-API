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
- **Swagger/OpenAPI** para documentação
- **Paginação** via `page` e `pageSize`
- **HATEOAS**: respostas incluem links de navegação (coleções e recursos) para facilitar descoberta de rotas
- **Camadas do projeto**
  - `Controllers/` – entrada HTTP e contratos REST
  - `Data/` – `DbContext`, mapeamentos e migrações
  - `Models/` – entidades de domínio e DTOs
  - `Migrations/` – versionamento de esquema (EF Core)

> Justificativa: A separação por camadas simplifica manutenção e testes, enquanto EF Core acelera o desenvolvimento seguro com Oracle. Swagger garante **transparência dos contratos** e acelera QA.

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

## 🔗 Endpoints (CRUD + Paginação + HATEOAS)

> **Paginação**: use `?page=1&pageSize=10`.  
> **HATEOAS**: as respostas incluem `links` com `rel`, `href` e `method` (exemplos abaixo).  
> **Status codes**: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `404 Not Found`, `409 Conflict` (quando aplicável).

### Pátios
- `GET /api/patios` – lista (paginação)
- `GET /api/patios/{id}` – detalhe
- `POST /api/patios` – cria
- `PUT /api/patios/{id}` – atualiza
- `DELETE /api/patios/{id}` – exclui

**Exemplo – criação**
```http
POST /api/patios
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
    {"rel":"self","href":"/api/patios/1","method":"GET"},
    {"rel":"update","href":"/api/patios/1","method":"PUT"},
    {"rel":"delete","href":"/api/patios/1","method":"DELETE"}
  ]
}
```

### Funcionários
- `GET /api/funcionarios`
- `GET /api/funcionarios/{id}`
- `POST /api/funcionarios`
- `PUT /api/funcionarios/{id}`
- `DELETE /api/funcionarios/{id}`

**Exemplo – criação**
```http
POST /api/funcionarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@mottu.com",
  "senha": "Senha@123",
  "patioId": 1
}
```

### Gerentes
- `GET /api/gerentes`
- `GET /api/gerentes/{id}`
- `POST /api/gerentes`
- `PUT /api/gerentes/{id}`
- `DELETE /api/gerentes/{id}`

**Exemplo – criação**
```http
POST /api/gerentes
Content-Type: application/json

{
  "funcionarioId": 1,
  "patioId": 1
}
```

---

## 🧪 Testes
Execute todos os testes do repositório:
```bash
dotnet test
```

---

## ✅ Checklist vs. Requisitos do Professor

- [x] **3 entidades** principais (Pátio, Funcionário, Gerente) **com justificativa** de domínio
- [x] **CRUD** completo para as 3 entidades
- [x] **Boas práticas REST**: recursos, verbos, status codes e validações
- [x] **Paginação** (`page`, `pageSize`) em coleções
- [x] **HATEOAS** para navegação entre recursos
- [x] **Swagger/OpenAPI** com descrição, parâmetros, exemplos e modelos
- [x] **Repositório GitHub público** com **README** claro
- [x] **Comando para rodar testes** (`dotnet test`)

> **Penalidades que este README ajuda a evitar**  
> -20 pts — falta de documentação Swagger • -100 pts — projeto não compila • -20 pts — sem README

---

## 📄 Licença
Uso acadêmico. Ajuste conforme a política da disciplina.

