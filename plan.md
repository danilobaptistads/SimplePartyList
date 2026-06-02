# SimplePartyList - Plano de Desenvolvimento

## Tecnologias

- .NET 9
- ASP.NET Core Web API
- Blazor Server (Interactive Server)
- Entity Framework Core
- SQLite
- ASP.NET Core Identity + JWT Bearer
- xUnit (TDD)

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
│   │   ├── Controllers/                  # (planejado)
│   │   │   ├── AuthController.cs
│   │   │   ├── ChosenListController.cs
│   │   │   ├── ItemController.cs
│   │   │   └── ChosenController.cs
│   │   ├── Program.cs                    # configurado (DbContext, Identity, CORS)
│   │   ├── appsettings.json              # connection string SQLite
│   │   ├── appsettings.Development.json
│   │   ├── Properties/
│   │   │   └── launchSettings.json
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
│   │   ├── Interfaces/                   # (planejado)
│   │   │   ├── IChosenListService.cs
│   │   │   ├── IItemService.cs
│   │   │   └── IChosenService.cs
│   │   ├── DTOs/                         ✅ LoginDto, RegisterDto
│   │   │   ├── LoginDto.cs
│   │   │   ├── RegisterDto.cs
│   │   │   └── ... (itens, escolhas)
│   │   └── core.csproj
│   │
│   ├── infrastructure/                   # EF Core, Repositories, Services
│   │   ├── Data/
│   │   │   ├── SimplePartyListContext.cs ✅
│   │   │   ├── DbInitializer.cs          ✅ (seed SplAdmin)
│   │   │   └── Database/                 ✅ (banco SQLite local)
│   │   ├── Services/                     # (planejado)
│   │   │   ├── ChosenListService.cs
│   │   │   ├── ItemService.cs
│   │   │   └── ChosenService.cs
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
│       │   │   └── List/                 # (planejado)
│       │   │       ├── Index.razor
│       │   │       └── Index.razor.cs
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor
│       │   │   ├── MainLayout.razor.css
│       │   │   ├── NavMenu.razor
│       │   │   └── NavMenu.razor.css
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── _Imports.razor
│       │   └── PopupConfirmacao.razor    # (planejado)
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
    │   ├── Services/                     # (planejado)
    │   │   ├── ChosenListServiceTests.cs
    │   │   ├── ItemServiceTests.cs
    │   │   └── ChosenServiceTests.cs
    │   └── Integration/                  # (planejado)
    │       └── PersistenceTests.cs
    └── tests.csproj
```

### Fluxo de Comunicação entre Projetos

```
Web (Blazor Server) ──HTTP──> API ──DI──> Infrastructure (Services) ──EF──> SQLite
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
- `ChosenListController`, `ItemController`, `ChosenController` exigem `[Authorize]`
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

### Etapa 3 - Persistência
- [x] Implementar repositórios (se necessário) — não necessário (EF Core direto)
- [x] ~~Implementar services~~ ✅ (ChosenList, Item, Chosen — todos implementados)
- [ ] Testes de integração com SQLite real

### Etapa 4 - API Controllers
- [ ] `AuthController` (register/login Identity + gerar JWT)
- [ ] `ChosenListController` (CRUD lista + geração link)
- [ ] `ItemController` (CRUD itens)
- [ ] `ChosenController` (submeter/confirmar/deletar escolha)
- [ ] Swagger/OpenAPI configurado

### Etapa 5 - Blazor Frontend
- [ ] Configurar `Program.cs` do Web (HttpClient apontando para API)
- [ ] **Dashboard Admin** (autenticado via Identity):
  - Criar/editar lista
  - Gerenciar itens (adicionar, editar, deletar)
  - Visualizar escolhas
  - Deletar escolha
- [ ] **Página pública** `/list/{guid}`:
  - Exibir itens disponíveis com cotas
  - Usuário marca itens desejados
  - Botão "Submeter"
- [ ] **Popup de confirmação** (2 etapas):
  - Etapa 1: input nome + "Prosseguir"
  - Etapa 2: confirmação com nome/itens + "Confirmar" / "Editar"
  - Confirmar → persiste e exibe nome na lista

### Etapa 6 - Expiração + Validações
- [ ] Verificar expiração ao acessar lista
- [ ] Bloquear submissão se expirado
- [ ] Validações de formulário (nome obrigatório, etc.)
- [ ] Feedback visual para o usuário (loading, erros, sucesso)

### Etapa 7 - Ajustes Finais
- [ ] UI responsiva (CSS básico ou Bootstrap)
- [ ] Testes end-to-end manuais
- [ ] Revisão geral de segurança
