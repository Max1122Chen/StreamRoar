---
name: c-style
description: C coding style for .c and .h files (ISO C11, modular naming, English comments). Use when implementing, refactoring, or reviewing C code. Prefer project-local coding-style docs and .clang-format when present; otherwise follow this skill. Do not use C++ syntax or STL in C-only projects.
---

# C Style

## When to Apply

- Writing or modifying C (`.c`, `.h`)
- Reviewing C changes or suggesting fixes
- **Do not** apply this skill to C++ sources (`.cpp`, `.hpp`) — use the `cpp-style` skill instead

## Priority Order

1. **Project docs** — e.g. `coding-style.md`, `CONTRIBUTING.md`, `AGENTS.md`, ADRs
2. **Project formatter** — `.clang-format` or editorconfig at repo root
3. **This skill** — when the repo has no stricter local rules

If project rules conflict with this skill, **follow the project**.

## Language & Standard

- Target **ISO C11** (`-std=c11`) unless the project specifies another standard
- Use **C standard library** and **documented OS / platform APIs** only, unless the project explicitly allows a third-party library
- **No C++** — do not compile `.c` as C++, do not use C++ keywords, references, classes, templates, or STL in C sources
- Prefer `stdbool.h` for boolean semantics when appropriate

## Files & Modules

- One **module** = one pair of `.c` + `.h` when practical; names aligned with responsibility
- **Source file names**: lowercase + underscores, match module name (e.g. `dns_table.c`)
- **Headers**: same basename as `.c`; location per project (`include/`, `src/`, etc.)
- **Include guards**: `PROJECT_MODULE_H` style — use the project's actual prefix (e.g. `DNS_RELAY_DNS_TABLE_H`), not a random string
- **Opaque types**: forward-declare `struct foo` in `.h`, define in `.c` when hiding implementation is useful
- **Internal helpers**: declare `static` at file scope in `.c`; do not expose in `.h` unless they are part of the public API

## Naming

### Principles

- **Readability first** — names express intent; avoid `tmp`, `buf2`, `do_it`
- **Module prefix** on public symbols (`<module>_<verb>`) to avoid global collisions
- **Domain abbreviations** are OK when standard for that protocol or API (`qname`, `rcode`, etc.)

### Functions

- Pattern: **`<module>_<verb>[_<object>]`**

```c
int dns_table_load(const char *path);
int id_map_insert(uint16_t upstream_id, const struct pending_query *q);
void debug_log(int level, const char *fmt, ...);
```

- Prefer **0 for success**, **non-zero** for errors (or a documented module `enum`); document any deviation
- One responsibility per function; split long functions

### Types

- Struct tags: **`struct <module>_<name>`** (or project-consistent `typedef`)
- Optional `typedef ... foo_t` — **pick one style per project** and stay consistent
- Enum **type** names: `snake_case` or `enum dns_rcode` per project
- Enum **constants**: **`UPPER_SNAKE_CASE`** with module prefix when it avoids clashes: `DNS_RCODE_OK`

### Variables

| Scope | Style | Example |
|-------|--------|---------|
| Locals | `snake_case` | `bytes_read`, `client_addr` |
| Parameters | `snake_case` | `const char *path` |
| File `static` | `snake_case`; optional `s_` prefix if project uses it | `s_debug_level` |
| True globals (avoid) | `g_` prefix if unavoidable | `g_config` |

- Loop indices: `i`, `j` OK; use descriptive names when nesting is deep
- Boolean-ish values: `int` or `bool` + `is_` / `has_` prefix: `is_valid`, `has_expired`

### Macros & Constants

```c
#define DNS_UDP_MAX_SIZE 512
#define DNS_RELAY_DEFAULT_PORT 53

enum { DNS_ID_MAP_TIMEOUT_MS = 5000 };
```

- **Macros**: `UPPER_SNAKE_CASE`
- Minimize macros with **side effects**; prefer `static inline` functions (C11) or ordinary functions for non-trivial logic

### Platform Code

- Isolate `WSA*`, `errno`, sockets, threads in **platform** or **adapter** modules
- Business logic calls **`platform_*`**, **`sock_*`**, or project-specific abstractions — avoid scattering raw OS calls

## Formatting (default when project silent)

- **Indent**: 4 spaces, no tab characters for indentation
- **Braces**: **Allman** — opening `{` on its own line for functions (match surrounding file if project differs)
- **Line length**: wrap around ~100 columns unless project specifies otherwise
- **Include order** (typical): system headers (`<...>`) → other libraries → **project** headers (`"..."`)

## Comments

- **English only** in `.c` / `.h` — `//`, `/* */`, file headers, and preprocessor notes
- **No non-English** in source comments unless the project doc explicitly overrides
- User-visible log strings: default **English**; localized strings only if the project requires it and documents where they live
- **Short and purposeful** — explain *why*, invariants, protocol edges; do not narrate obvious code
- Remove wrong or stale comments

## Error Handling & Resources

- Every `malloc` / `calloc` / `realloc`: **check** return value
- Prefer **`goto cleanup`** or a small structured pattern so all paths free resources; avoid duplicated cleanup
- Document which functions **transfer ownership** of pointers
- On Windows sockets: respect `WSAStartup` / `WSACleanup` pairing; log `WSAGetLastError()` where useful

## Third-Party & Course / Embedded Constraints

- Some projects **forbid** third-party C libraries — do not add glib, uthash, cJSON, etc. unless **explicitly allowed**
- Avoid large copy-paste blocks; **extract functions** when two regions only differ by data
