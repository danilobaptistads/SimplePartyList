# SimplePartyList — Plano de Desenvolvimento

## Status Atual

| Item | Status |
|------|--------|
| **Branch** | `fix/bugs-sobre` (3 alterações não commitadas) |
| **Build** | ✅ **0 erros** |
| **Testes** | ✅ **89 passando** (86 existentes + 3 NavigationTests) |
| **Último commit** | `7fba723` — "fix: substitui NavigationState por query string returnUrl no Sobre" |
| **Servidor** | `http://localhost:5193` |
| **Produção** | `https://simplepartylist.runasp.net` (sem as correções recentes) |

## Tecnologias

- .NET 9 — Monolito ASP.NET Core (Web API + Blazor Server Interactive Server)
- Entity Framework Core + Npgsql (PostgreSQL — Supabase)
- ASP.NET Core Identity + JWT Bearer
- xUnit + WebApplicationFactory + Moq (testes)
- GitHub Actions + WebDeploy (deploy automático via `main`)

---

## Linha do Tempo (Sessão Atual — fix/bugs-sobre)

### Problemas Identificados

1. **Navegação Sobre quebrada** — clicar "SOBRE" a partir da lista pública (`/list/{guid}`) redirecionava para `/login`
2. **`PublicLayout.razor` — diretivas na ordem errada** — `@inject` antes de `@inherits` impedia injeção do `NavigationManager`
3. **Navegação inconsistente** — métodos com `@onclick` + `NavigateTo()` causavam exceções no circuito Blazor

### Correções Aplicadas

#### 1. Ordem de diretivas no `PublicLayout.razor`
- **Antes:** `@inject` (L1), `@inherits` (L2)
- **Depois:** `@inherits` (L1), `@inject` (L2)

#### 2. Substituição do click handler por href computado
- **Antes:** `<a href="#" @onclick="OpenAbout" @onclick:preventDefault="true">`
  - `OpenAbout()` chamava `Navigation.NavigateTo()` em runtime
  - Exceções no evento derrubavam o circuito → caía na rota `/` → `Home.razor` redirecionava para `/login`
- **Depois:** `<a href="@GetSobreUrl()">`
  - `GetSobreUrl()` computa o href estático em tempo de renderização
  - Blazor intercepta o clique no `<a>` nativamente (sem `@onclick`)
  - Zero exceções possíveis em runtime

#### 3. Criação do `NavigationHelper.cs`
- `src/web/Services/NavigationHelper.cs` — classe injetável com:
  - `IrParaSobreComRetorno()` — navega com `returnUrl` dinâmico
  - `IrParaSobre()` — navega direto para `/sobre`
- Registrado em `Program.cs` como `AddScoped`
- Testado via `TestNavigationManager` mock

#### 4. Testes de navegação (3 novos)
- `tests/allTests/Pages/NavigationTests.cs`:
  - `De_PaginaPublicaLista_NavegaParaSobreComReturnUrl` ✅
  - `De_Dashboard_NavegaParaSobre` ✅
  - `De_EventoDetalhe_NavegaParaSobre` ✅

#### 5. Ajuste de texto no `Sobre.razor`
- Correção textual: "essa jornada", "ao Ethan"

### Erro Revertido

- `@rendermode InteractiveServer` adicionado ao `PublicLayout.razor` → causou:
  ```
  InvalidOperationException: Cannot pass the parameter 'Body' to component
  'PublicLayout' with rendermode 'InteractiveServerRenderMode'...
  ```
- Revertido imediatamente. Layout herda render mode da página (sem `@rendermode` explícito).

---

## Arquivos Alterados (não commitados)

| Arquivo | Tipo de mudança |
|---|---|
| `src/web/Components/Layout/PublicLayout.razor` | `@inherits` antes de `@inject` + `@GetSobreUrl()` no lugar do click handler |
| `src/web/Components/Pages/Sobre.razor` | Correção textual |
| `src/web/Components/_Imports.razor` | Added `@using SimplePartyList.Web.Services` |
| `src/web/Program.cs` | Added `using` + `AddScoped<NavigationHelper>()` |
| `src/web/Services/NavigationHelper.cs` | **Novo** — helper de navegação testável |
| `tests/allTests/Pages/NavigationTests.cs` | **Novo** — 3 testes + `TestNavigationManager` |

---

## Bug Conhecido em Produção

**`PublicLayout.razor` em produção** ainda tem:
- `@inject` antes de `@inherits`
- Click handler com `@onclick` em vez de href computado

Isso causa o mesmo problema: Sobre → `/login`. A correção está na branch `fix/bugs-sobre` esperando commit + merge + deploy.

---

## Próximos Passos

### Imediatos
1. Commit das alterações em `fix/bugs-sobre`
2. Merge para `develop`
3. PR `develop → main`
4. Deploy automático (GitHub Action + WebDeploy)
5. Testar Sobre em produção

### Pendentes (próxima sessão)
- [ ] Trocar `ProtectedSessionStorage` por `ProtectedLocalStorage` em `AdminAuthHelper.cs`
- [ ] Corrigir vulnerabilidades ZAP no `Program.cs`:
  - Unificar headers CSP duplicados
  - Adicionar `CookieSecurePolicy.Always` no antiforgery
  - Remover headers `Server` / `X-Powered-By`
  - Aumentar HSTS `max-age` para 31536000
- [ ] Diagnosticar sidebar mobile (100% em vez de 260px)
- [ ] Testes end-to-end manuais

---

## Estrutura de Pastas (atualizada)

```
SimplePartyList/
├── SimplePartyList.sln
│
├── src/
│   ├── web/                              # Monolito (API + Blazor)
│   │   ├── Components/
│   │   │   ├── Pages/
│   │   │   │   ├── Home.razor
│   │   │   │   ├── Sobre.razor
│   │   │   │   ├── Counter.razor
│   │   │   │   ├── Weather.razor
│   │   │   │   ├── Error.razor
│   │   │   │   ├── Admin/
│   │   │   │   │   ├── Dashboard.razor
│   │   │   │   │   ├── CriarEvento.razor
│   │   │   │   │   ├── EventoDetalhe.razor
│   │   │   │   │   ├── Login.razor
│   │   │   │   │   └── AdminAuthHelper.cs
│   │   │   │   └── List/
│   │   │   │       ├── Index.razor
│   │   │   │       └── ListPageHelper.cs
│   │   │   ├── Layout/
│   │   │   │   ├── MainLayout.razor
│   │   │   │   ├── BlankLayout.razor
│   │   │   │   ├── PublicLayout.razor    ← corrigido
│   │   │   │   └── NavMenu.razor
│   │   │   ├── App.razor
│   │   │   ├── Routes.razor
│   │   │   ├── _Imports.razor
│   │   │   └── PopupConfirmacao.razor
│   │   ├── Controllers/
│   │   │   └── AuthController.cs
│   │   ├── Endpoints/
│   │   │   ├── EventEndpoints.cs
│   │   │   ├── ChosenListEndpoints.cs
│   │   │   ├── ItemEndpoints.cs
│   │   │   └── ChosenEndpoints.cs
│   │   ├── Services/
│   │   │   ├── NavigationHelper.cs        ← novo
│   │   │   ├── NavigationContextService.cs
│   │   │   ├── TokenStore.cs
│   │   │   └── SecurityHeadersMiddleware.cs
│   │   ├── Program.cs
│   │   └── web.csproj
│   │
│   ├── core/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   ├── DTOs/
│   │   └── core.csproj
│   │
│   └── infrastructure/
│       ├── Data/
│       │   ├── SimplePartyListContext.cs
│       │   └── DbInitializer.cs
│       ├── Services/
│       └── infrastructure.csproj
│
└── tests/
    ├── allTests/
    │   ├── Pages/
    │   │   ├── ListPageTests.cs
    │   │   └── NavigationTests.cs          ← novo
    │   ├── Services/
    │   ├── Endpoints/
    │   └── Integration/
    └── tests.csproj
```
