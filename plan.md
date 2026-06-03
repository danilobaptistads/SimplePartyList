# SimplePartyList - Plano de Desenvolvimento

## Tecnologias

- .NET 9
- ASP.NET Core Web API
- Blazor Server (Interactive Server)
- Entity Framework Core + Npgsql (PostgreSQL)
- Supabase (PostgreSQL hosted)
- ASP.NET Core Identity + JWT Bearer
- xUnit (TDD) — **86 testes** (39 Services + 5 Auth + 10 Event + 5 ChosenList + 11 Item + 8 Chosen + 6 Blazor helper + 2 integração)

---

## Regras do Fluxo de Trabalho

1. **Nunca commitar sem confirmação do usuário** — sempre perguntar antes de commitar.
2. **Nunca alterar arquivos existentes sem confirmação** — toda modificação em arquivo existente deve ser aprovada antes.
3. Após aprovação, executar a alteração e aguardar nova instrução.

---

## Estrutura de Pastas

```
SimplePartyList/
├── SimplePartyList.sln
│
├── src/
│   ├── api/                              # ASP.NET Core Web API
│   │   ├── Controllers/                  ✅ AuthController (único)
│   │   │   └── AuthController.cs
│   │   ├── Endpoints/                    # Minimal API
│   │   │   ├── EventEndpoints.cs
│   │   │   ├── ChosenListEndpoints.cs
│   │   │   ├── ItemEndpoints.cs          ✅
│   │   │   └── ChosenEndpoints.cs        ✅
│   │   ├── Program.cs                    # configurado (DbContext, Identity, CORS)
│   │   ├── ProgramPublic.cs              # classe parcial para testes
│   │   ├── appsettings.json              # config (connection string em User Secrets)
│   │   ├── appsettings.Development.json
│   │   ├── Properties/
│   │   │   ├── launchSettings.json
│   │   │   └── AssemblyInfo.cs           # InternalsVisibleTo para testes
│   │   ├── SimplePartyList.API.http
│   │   └── api.csproj
│   │
│   ├── core/                             # Entidades, Interfaces, DTOs
│   │   ├── Entities/                     ✅ Admin, Event, ChosenList, Item, Chosen
│   │   │   ├── Admin.cs
│   │   │   ├── Event.cs
│   │   │   ├── ChosenList.cs
│   │   │   ├── Item.cs
│   │   │   └── Chosen.cs
│   │   ├── Interfaces/                   ✅
│   │   │   ├── IChosenListService.cs
│   │   │   ├── IItemService.cs
│   │   │   ├── IChosenService.cs
│   │   │   └── IEventService.cs
│   │   ├── DTOs/                         ✅ LoginDto, RegisterDto, CreateEventDto,
│   │   │   ├── LoginDto.cs               │     UpdateEventDto, AdminEventResponseDto,
│   │   │   ├── RegisterDto.cs            │     AuthResponseDto, PublicListResponseDto,
│   │   │   ├── CreateEventDto.cs         │     ItemDto, CreateItemDto, UpdateItemDto,
│   │   │   ├── UpdateEventDto.cs         │     ChosenResponseDto, SubmitChosenDto
│   │   │   ├── AuthResponseDto.cs
│   │   │   ├── AdminEventResponseDto.cs
│   │   │   ├── PublicListResponseDto.cs
│   │   │   ├── ItemDto.cs
│   │   │   ├── CreateItemDto.cs
│   │   │   ├── UpdateItemDto.cs
│   │   │   ├── ChosenResponseDto.cs
│   │   │   └── SubmitChosenDto.cs
│   │   └── core.csproj
│   │
│   ├── infrastructure/                   # EF Core, Repositories, Services
│   │   ├── Data/
│   │   │   ├── SimplePartyListContext.cs ✅
│   │   │   └── DbInitializer.cs          ✅ (seed SplAdmin + migrate)
│   │   ├── Services/                     ✅
│   │   │   ├── ChosenListService.cs
│   │   │   ├── ItemService.cs
│   │   │   ├── ChosenService.cs
│   │   │   └── EventService.cs
│   │   ├── Migrations/                   ✅ InitialCreate
│   │   └── infrastructure.csproj
│   │
│   └── web/                              # Blazor Server
│       ├── Components/
│       │   ├── Pages/
│       │   │   ├── Home.razor
│       │   │   ├── Counter.razor
│       │   │   ├── Weather.razor
│       │   │   ├── Error.razor
│       │   │   └── List/                 ✅
│       │   │       ├── Index.razor
│       │   │       └── ListPageHelper.cs
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor
│       │   │   ├── MainLayout.razor.css
│       │   │   ├── NavMenu.razor
│       │   │   ├── NavMenu.razor.css
│       │   │   └── PublicLayout.razor    ✅
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── _Imports.razor
│       │   └── PopupConfirmacao.razor    ✅
│       ├── wwwroot/
│       │   ├── app.css
│       │   ├── favicon.png
│       │   └── lib/bootstrap/dist/...
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── web.csproj
│
└── tests/                                # xUnit Tests
    ├── allTests/                         # pasta que abriga os testes
    │   ├── Services/                     ✅
    │   │   ├── ChosenListServiceTests.cs
    │   │   ├── ItemServiceTests.cs
    │   │   ├── ChosenServiceTests.cs
    │   │   └── EventServiceTests.cs
    │   ├── Controllers/                  ✅
    │   │   └── AuthControllerTests.cs
    │   ├── Endpoints/                    ✅
    │   │   ├── EventEndpointTests.cs
    │   │   ├── ChosenListEndpointTests.cs
    │   │   ├── ItemEndpointTests.cs
    │   │   └── ChosenEndpointTests.cs
    │   ├── Pages/                       ✅
    │   │   └── ListPageTests.cs
    │   └── Integration/                  ✅
    │       └── PersistenceTests.cs
    └── tests.csproj
```

### Fluxo de Comunicação entre Projetos

```
Web (Blazor Server) ──HTTP──> API ──DI──> Infrastructure (Services) ──EF──> PostgreSQL (Supabase)
                                    Core (entities/interfaces/DTOs)
```

- `api` referencia `core` + `infrastructure`
- `web` referencia `core` (apenas DTOs) e chama a `api` via `HttpClient`
- `infrastructure` referencia `core`
- `tests` referencia `core` + `infrastructure`

---

## Modelagem (Entidades)

### Admin (extends IdentityUser)

| Propriedade | Tipo |
|---|---|
| `Id` | `string` (herdado do IdentityUser) |
| `Name` | `string` |
| `Events` | `ICollection<Event>` |

### Event

| Propriedade | Tipo |
|---|---|
| `EventId` | `Guid` |
| `Name` | `string` |
| `Date` | `DateTime` (data e hora combinadas) |
| `AdminId` | `string` |
| `ChosenListId` | `Guid` |

### ChosenList

| Propriedade | Tipo |
|---|---|
| `ChosenListId` | `Guid` |
| `ListUrl` | `Guid` |
| `Expire` | `DateTime` |
| `Items` | `ICollection<Item>` |
| `Chosens` | `ICollection<Chosen>` |

### Item

| Propriedade | Tipo |
|---|---|
| `ItemId` | `Guid` |
| `Name` | `string` |
| `MaxQuantity` | `int?` (null = sem restrição) |
| `ChosenListId` | `Guid` |

### Chosen

| Propriedade | Tipo |
|---|---|
| `ChosenId` | `Guid` |
| `GuestName` | `string` |
| `ItemName` | `string` |
| `ChosenListId` | `Guid` |

### Relacionamentos

```
Admin  1──*  Event
Event  1──1  ChosenList
ChosenList 1──* Item
ChosenList 1──* Chosen
```

---

## Segurança (Ownership)

> **Regra**: Todo endpoint admin (PUT/DELETE/POST de recursos) deve verificar se o recurso pertence ao `AdminId` do token JWT. Caso contrário, retorna `403 Forbid`.

- Event PUT/DELETE — verifica `ev.AdminId == adminId` ✅
- Event POST — usa `adminId` do token (recurso criado já pertence ao admin) ✅
- Item POST/PUT/DELETE (4D) — verificar ownership via Event → AdminId
- ChosenListEndpoints — públicos (guest), sem auth ✅
- Chosen GET/DELETE (4E) — GET verifica ownership via Event → AdminId; DELETE no admin (POST guest)
- Item GET/DELETE (4D) — verifica ownership via Event → AdminId; DELETE 409 se item tem Chosens

## Regras de Negócio

1. **Admin** autenticado via Identity cria um evento e um `ChosenList`, unica, vinculada a um `Event`
2. Ao criar a lista, gera-se `ListUrl = Guid.NewGuid()` e `Expire = Event.Date.AddDays(1)`
3. **Admin** pode adicionar itens novos ou reutilizar itens existentes
4. **Itens** podem ter `MaxQuantity = null` (sem restrição) ou valor numérico (cota limitada)
5. **Admin** pode editar/deletar apenas itens **não escolhidos** (sem `Chosen` vinculado)
6. **Usuário** acessa a lista via link público `/list/{guid}`, visualiza itens disponíveis e marca os desejados
7. Após submeter a escolha, aparece **popup em 2 etapas**:
   - **Etapa 1:** digitar nome do convidado + botão "Prosseguir"
   - **Etapa 2:** confirmação exibindo nome + itens selecionados, com botões "Confirmar" e "Editar"
   - "Editar" → volta para tela de seleção
   - "Confirmar" → persiste `Chosen` com `GuestName` e exibe o nome na lista
8. **Apenas o Admin** pode deletar uma escolha (via sistema). O usuário solicita fora do sistema.
9. Ao deletar uma escolha, a cota do item é liberada (se houver)
10. **Link expirado** (`DateTime.UtcNow > Expire`) → bloqueia novas escolhas, apenas visualização

---

## Autenticação (JWT Bearer)

- Admin registra/login via `AuthController`
- API retorna um **JWT token** com claims (UserId, Email, Name)
- Token expira conforme `Jwt.ExpireMinutes` (ex: 60 min)
- `EventEndpoints`, `ItemEndpoints`, `ChosenEndpoints` exigem `[Authorize]`
- Rota pública: apenas `POST /api/auth/login` e `POST /api/auth/register`
- Swagger configurado com `AddSecurityDefinition` (Bearer) para testes

**Config (appsettings.json):**
```json
"Jwt": {
  "Issuer": "SimplePartyList",
  "Audience": "SimplePartyList",
  "ExpireMinutes": 60
}
```
> A chave `Jwt:Key` é armazenada no **User Secrets** em desenvolvimento e via variável de ambiente `Jwt__Key` em produção.

**Pacote NuGet:** `Microsoft.AspNetCore.Authentication.JwtBearer`

---

## Etapas de Desenvolvimento

### Etapa 1 - Setup + Modelagem
- [x] Criar solution `SimplePartyList.sln`
- [x] Criar projetos: `API`, `Core`, `Infrastructure`, `Web`, `Tests`
- [x] Instalar pacotes NuGet (EF Core SQLite, Identity, etc.)
- [x] Criar classes de entidade (`Admin`, `Event`, `ChosenList`, `Item`, `Chosen`)
- [x] Modelagem finalizada (relacionamentos definidos)
- [x] Criar `SimplePartyListContext` herdando `IdentityDbContext<Admin>`
- [x] Configurar Fluent API no `OnModelCreating`
- [x] Criar migration `InitialCreate` e aplicar ao banco SQLite
- [x] Configurar `Program.cs` da API (DbContext, Identity, CORS, Controllers)
- [x] Configurar connection string SQLite em `appsettings.json`
- [x] Remover `Class1.cs` placeholder da Infrastructure
- [x] Criar `DbInitializer` com seed automático do `SplAdmin` (user: `spladmin` / email: `spladmin@spl.com` / senha: `SplAdmin@123`)
- [x] Adicionar `Microsoft.AspNetCore.Authentication.JwtBearer` (pacote NuGet)
- [x] Configurar JWT no `appsettings.json` (Issuer, Audience, ExpireMinutes) + Key via User Secrets
- [x] Configurar `AddAuthentication().AddJwtBearer()` no `Program.cs` da API
- [x] Corrigir referências do `tests.csproj` (`..\..\src\` → `..\src\`)

### Etapa 2 - TDD (Testes dos Services) — Ciclo por Service

Cada service segue o fluxo:
1. Criar **Interface** no `Core`
2. Criar **Testes** (xUnit + InMemory) — você revisa
3. Após aprovação, criar a **implementação do Service** na Infrastructure

#### 2A - ChosenListService
- [x] Criar `IChosenListService.cs` (interface)
- [x] Criar `ChosenListServiceTests.cs` (7 testes — InMemory)
- [x] Criar `ChosenListService.cs` (implementação)

#### 2B - ItemService
- [x] Criar `IItemService.cs` (interface) — `AddNewAsync`, `SearchByNameAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`, `GetByListUrlAsync`
- [x] Criar `ItemServiceTests.cs` com 11 testes (InMemory):
  - `AddNewAsync_ShouldCreateItem_WithCota` / `WithoutCota`
  - `SearchByNameAsync_ShouldReturnMatchingItems` / `Empty_WhenNoMatch`
  - `GetByIdAsync_ShouldReturnItem` / `Null_WhenNotFound`
  - `UpdateAsync_ShouldPersistChanges` / `ShouldThrow_WhenNotFound`
  - `DeleteAsync_ShouldRemoveItem` / `ShouldThrow_WhenNotFound`
  - `GetByListUrlAsync_ShouldReturnItems` / `Empty_WhenNoItems`
- [x] ~~*Aguardar revisão → criar `ItemService.cs`*~~ ✅ implementado e merged

#### 2C - ChosenService
- [x] Criar `IChosenService.cs` (interface) — `SubmitAsync`, `DeleteAsync`, `GetByChosenListIdAsync`
- [x] Criar `ChosenServiceTests.cs` com 8 testes (InMemory):
  - `SubmitAsync_ShouldCreateChosen`
  - `SubmitAsync_ShouldThrow_WhenListExpired`
  - `SubmitAsync_ShouldThrow_WhenQuotaExceeded`
  - `SubmitAsync_ShouldThrow_WhenItemNotFound`
  - `DeleteAsync_ShouldRemoveChosen` / `ShouldThrow_WhenNotFound`
  - `GetByChosenListIdAsync_ShouldReturnChosens` / `Empty_WhenNoChosens`
- [x] ~~*Aguardar revisão → criar `ChosenService.cs`*~~ ✅ implementado e merged

#### 2D - EventService
- [x] Criar `IEventService.cs` (interface) — `CreateAsync`, `GetByIdAsync`, `GetByAdminIdAsync`, `UpdateAsync`, `DeleteAsync`
- [x] Criar `EventServiceTests.cs` com 8 testes (InMemory)
- [x] ~~*Aguardar revisão → criar `EventService.cs`*~~ ✅ implementado e merged
- [x] Merge `feature/eventservice` → `develop`
- [x] Deletar branch `feature/eventservice`

### Etapa 3 - Persistência
- [x] Implementar repositórios (se necessário) — não necessário (EF Core direto)
- [x] ~~Implementar services~~ ✅ (ChosenList, Item, Chosen, Event — todos implementados)
- [x] Migração SQLite → Supabase PostgreSQL
- [x] Testes de integração (InMemory) — 10 testes em `PersistenceTests.cs`
- [x] Merge `feature/integration-tests` → `develop`
- [x] Deletar branch `feature/integration-tests`

### Etapa 4 - API Endpoints (Minimal API)

Sequência: **DTO(s) → Endpoints (Minimal API) → Testes**

> **AuthController** mantido como controller-based. **4B-4E** usarão Minimal API puro via `src/api/Endpoints/`.
>
> **Separação Guest vs Admin via DTOs** — o modelo (`ChosenList`) é único. A diferença fica nos DTOs de resposta:
> - **Guest** (`GET /api/lists/{listUrl}`) → `PublicListResponseDto` (Items + quantidade disponível, nome/data do Event). Sem Chosens, sem Expire.
> - **Admin** (`GET /api/events/{id}`) → `AdminEventResponseDto` (Chosens, ChosenListId, dados do Event).

#### 4A - AuthController (controller-based)
- [x] `AuthResponseDto` — `Token` (string), `Expire` (DateTime)
- [x] `AuthController` — `POST /api/auth/register`, `POST /api/auth/login`
- [x] `AuthControllerTests` — 5 testes (Moq)

#### 4B - EventEndpoints (Minimal API)
- [x] `CreateEventDto`, `UpdateEventDto`, `AdminEventResponseDto`
- [x] `EventEndpoints.cs` — POST, GET (lista), GET (id), PUT, DELETE
- [x] Ownership check: PUT/DELETE verificam `ev.AdminId == adminId` (403 se não dono)
- [x] `EventEndpointTests` — 10 testes (WebApplicationFactory)
- [x] `Program.cs` — `MapEventEndpoints()`, `if Testing` para InMemory
- [x] `DbInitializer.cs` — `IsRelational()` check
- [x] `api.csproj` — `InMemory` package, `ProgramPublic.cs`, `AssemblyInfo.cs`
- [x] `tests.csproj` — `FrameworkReference`, `api.csproj`, `Mvc.Testing`

#### 4C - ChosenListEndpoints (Minimal API, público)
- [x] `PublicListResponseDto` (guest)
- [x] `ItemDto` (guest: ItemId, Name, MaxQuantity, ChosenCount)
- [x] `ChosenListEndpoints.cs` — `GET /api/lists/{listUrl}`, `GET /api/lists/{listUrl}/expired`
- [x] `ChosenListEndpointTests` — 5 testes (WebApplicationFactory)
- [x] Merge `feature/eventendpoints` → `develop` (4B + 4C juntos)
- [x] Deletar branch `feature/eventendpoints`

#### 4D - ItemEndpoints
- [x] `CreateItemDto`, `UpdateItemDto`
- [x] `ItemEndpoints.cs` — POST, GET (lista), GET (id), PUT, DELETE
- [x] Ownership checks em POST/PUT/DELETE (Item → ChosenList → Event → AdminId)
- [x] DELETE 409 Conflict se item tem Chosens vinculados
- [x] `IItemService.GetByChosenListIdAsync`
- [x] `ItemEndpointTests` — 11 testes (WebApplicationFactory)
- [x] Merge `feature/itemendpoints` → `develop`
- [x] Deletar branch `feature/itemendpoints`

#### 4E - ChosenEndpoints
- [x] `SubmitChosenDto`, `ChosenResponseDto`
- [x] `ChosenEndpoints.cs` — GET /api/events/{eventId}/chosens (admin, ownership), POST /api/lists/{listUrl}/chosens (guest), DELETE /api/chosens/{id} (admin, ownership)
- [x] `IChosenService.GetByIdAsync`
- [x] `ChosenEndpointTests` — 8 testes (WebApplicationFactory)
- [x] Merge `feature/chosenendpoints` → `develop`
- [x] Deletar branch `feature/chosenendpoints`

#### Swagger
- [x] Configurado via `AddOpenApi()` + `MapOpenApi()`

### Etapa 5 - Blazor Frontend (Página Pública)
- [x] Configurar `Program.cs` do Web (HttpClient apontando para API)
- [x] `ListPageHelper.cs` — 3 métodos testáveis: `CarregarListaAsync`, `VerificarExpiracaoAsync`, `SubmeterEscolhaAsync`
- [x] `PublicLayout.razor` — header verde + body sem sidebar
- [x] **Página pública** `/list/{guid}`:
  - Passo 1: input nome completo
  - Passo 2: cards de itens com checkbox + barra de progresso de cotas
  - Botão "Submeter" com loading spinner
  - Sucesso: nome convidado + itens + botão voltar
  - Validação visual: mensagens abaixo do botão (só após interagir)
- [x] **Popup de confirmação** (`PopupConfirmacao.razor`):
  - Exibe nome e itens selecionados
  - Botões "Confirmar" e "Editar"
- [x] CSS customizado (cards bege com fitas, progresso, botão verde petróleo, hints, fitas decorativas)
- [x] Fitas decorativas (washi tape): 5 classes CSS individuais, posicionamento absoluto, clip-path
- [x] Pseudo-elementos cinzas removidos, padding dos cards aumentado
- [x] Event header com nome e data do evento
- [ ] **Dashboard Admin** (autenticado) — listar/criar/editar eventos, gerenciar itens, ver/deletar escolhas
- [x] `ListPageTests` — 6 testes Moq do helper
- [ ] **Dashboard Admin** (autenticado) — listar/criar/editar eventos, gerenciar itens, ver/deletar escolhas

### Etapa 6 - Expiração + Validações
- [x] Verificar expiração ao acessar lista (API + frontend)
- [x] Bloquear submissão se expirado (desabilitar inputs + botão)
- [x] Validações de formulário (nome + itens obrigatórios, só após interagir)
- [x] Feedback visual para o usuário (loading, erros, sucesso, spinner)
- [ ] Validação server-side no submit (se expirou entre abrir e submeter)

### Etapa 7 - Ajustes Finais
- [ ] UI responsiva (CSS básico ou Bootstrap)
- [ ] Testes end-to-end manuais
- [ ] Revisão geral de segurança
