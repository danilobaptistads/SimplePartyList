# SimplePartyList - Plano de Desenvolvimento

## Tecnologias

- .NET 9
- ASP.NET Core Web API
- Blazor Server (Interactive Server)
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- xUnit (TDD)

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
│   │   ├── Program.cs                    # template weather forecast (falta configurar)
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── SimplePartyList.API.http
│   │   └── api.csproj
│   │
│   ├── core/                             # Entidades, Interfaces, DTOs
│   │   ├── Entities/                     # (planejado)
│   │   │   ├── Admin.cs
│   │   │   ├── Event.cs
│   │   │   ├── ChosenList.cs
│   │   │   ├── Item.cs
│   │   │   └── Chosen.cs
│   │   ├── Interfaces/                   # (planejado)
│   │   │   ├── IChosenListService.cs
│   │   │   ├── IItemService.cs
│   │   │   └── IChosenService.cs
│   │   ├── DTOs/                         # (planejado)
│   │   │   ├── CreateItemDto.cs
│   │   │   ├── SubmitChoiceDto.cs
│   │   │   ├── ConfirmChoiceDto.cs
│   │   │   └── ...
│   │   └── core.csproj
│   │
│   ├── infrastructure/                   # EF Core, Repositories, Migrations
│   │   ├── Data/                         # (planejado)
│   │   │   ├── AppDbContext.cs
│   │   │   └── Configurations/
│   │   │       ├── AdminConfiguration.cs
│   │   │       ├── EventConfiguration.cs
│   │   │       ├── ChosenListConfiguration.cs
│   │   │       ├── ItemConfiguration.cs
│   │   │       └── ChosenConfiguration.cs
│   │   ├── Repositories/                 # (planejado)
│   │   │   ├── ChosenListRepository.cs
│   │   │   ├── ItemRepository.cs
│   │   │   └── ChosenRepository.cs
│   │   ├── Services/                     # (planejado)
│   │   │   ├── ChosenListService.cs
│   │   │   ├── ItemService.cs
│   │   │   └── ChosenService.cs
│   │   ├── Migrations/                   # (planejado)
│   │   ├── Class1.cs                     # placeholder (remover)
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
    ├── UnitTest1.cs                      # placeholder (remover)
    └── tests.csproj
```

### Fluxo de Comunicação entre Projetos

```
Web (Blazor Server) ──HTTP──> API ──DI──> Infrastructure ──EF──> SQLite
                                    Core (entities/interfaces)
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
| `Items` | `ICollection<Item>` |

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
Admin  1──*  Item
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

## Etapas de Desenvolvimento

### Etapa 1 - Setup + Modelagem
- [x] Criar solution `SimplePartyList.sln`
- [x] Criar projetos: `API`, `Core`, `Infrastructure`, `Web`, `Tests`
- [x] Instalar pacotes NuGet (EF Core SQLite, Identity, etc.)
  - ✅ `Infrastructure`: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` + `Microsoft.EntityFrameworkCore.Sqlite` v9.0.0
  - ✅ `API`: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` + `Microsoft.AspNetCore.OpenApi` v9.0.0
  - ✅ `Tests`: `xunit` v2.9.2, `Moq` v4.20.72, `coverlet.collector` v6.0.2, `Microsoft.EntityFrameworkCore.InMemory` v9.0.0, `Microsoft.NET.Test.Sdk` v17.11.1
  - ❌ `Core` e `Web` sem pacotes ainda
- [x] Criar classes de entidade (`Admin`, `Event`, `ChosenList`, `Item`, `Chosen`)
- [x] Modelagem finalizada (relacionamentos definidos)
- [ ] Criar `AppDbContext` herdando `IdentityDbContext<Admin>`
- [ ] Configurar Fluent API nas `Configurations`
- [ ] Criar migration inicial
- [ ] Configurar `Program.cs` da API (DI, DbContext, Identity, Swagger) — *atualmente está o template weather forecast*

### Etapa 2 - TDD (Testes dos Services)
- [ ] Configurar xUnit + Moq + InMemory provider — *pacotes já instalados, mas sem testes escritos*
- [ ] **`ChosenListServiceTests`**:
  - Criar lista → gera GUID + data de expiração correta
  - Obter lista por link GUID
  - Link expirado → bloqueia escolhas
- [ ] **`ItemServiceTests`**:
  - Adicionar item novo
  - Adicionar item existente (reuso)
  - Editar item não escolhido
  - Deletar item não escolhido
  - Bloquear edição/deleção de item escolhido
  - Cota: respeitar `MaxQuantity`
- [ ] **`ChosenServiceTests`**:
  - Submeter escolha (sem nome ainda)
  - Confirmar escolha (associa `GuestName`)
  - Bloquear escolha se cota estourada
  - Bloquear escolha se lista expirada
  - Deletar escolha (admin only) → libera cota

### Etapa 3 - Persistência
- [ ] Implementar `AppDbContext` com DbSets e Fluent API
- [ ] Implementar repositórios
- [ ] Implementar services (`ChosenListService`, `ItemService`, `ChosenService`)
- [ ] Testes de integração com SQLite real

### Etapa 4 - API Controllers
- [ ] `AuthController` (register/login Identity)
- [ ] `ChosenListController` (CRUD lista + geração link)
- [ ] `ItemController` (CRUD itens)
- [ ] `ChosenController` (submeter/confirmar/deletar escolha)
- [ ] Swagger/OpenAPI configurado
- [ ] Limpar placeholder `Class1.cs` da Infrastructure

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
