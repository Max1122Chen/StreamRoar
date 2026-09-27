---
name: cpp-style
description: C++ coding style and conventions for writing or editing .cpp, .h, .hpp files. Use when implementing, refactoring, or reviewing C++ code. Prefer project-local .clang-format or docs when present; otherwise follow this skill. Aligns with minEngine-style naming (m_/s_ members, PascalCase). Comments in English only.
---

# C++ Style

## When to Apply

- Writing or modifying C++ (`.cpp`, `.h`, `.hpp`, `.cc`, `.cxx`)
- Reviewing C++ changes or suggesting fixes

## Priority Order

1. **Project docs** — e.g. `coding-style.md`, `CONTRIBUTING.md`, `AGENTS.md`
2. **Project formatter** — `.clang-format` in the repo root
3. **This skill** — when the repo has no stricter local rules

If project rules conflict with this skill, **follow the project**.

## Language & Standard

- Prefer **modern C++ (C++17 or newer)** unless the project pins an older standard
- Match the project's CMake / toolchain standard when set
- Do not mix C-only style into C++ projects (no `malloc`/`free` in new C++ code; use RAII)
- In **C-only** projects (`.c` / C11), do not apply C++ syntax — follow that project's C style doc instead

## Naming

### General

| Entity | Style | Example |
|--------|--------|---------|
| Namespace | Match repo (often `camelCase` / `PascalCase`) | `minEngine` |
| Types (`class`, `enum`, `enum class`) | `PascalCase` | `RenderPipeline`, `ShaderStage` |
| `struct` (plain data / POD-style) | `PascalCase` type name | `MIRValue`, `BuildContext` |
| Functions / methods | `PascalCase` | `LoadAsset`, `IsValid` |
| Local variables | `camelCase` | `bytesRead`, `ownedBlock` |
| Parameters | `camelCase`; `in`/`out` prefix when helpful | `inKind`, `outError` |
| Constants / macros | `UPPER_SNAKE_CASE` for macros; clear `constexpr`/`const` names | `MaxCascadeCount` |
| Enum values | `PascalCase`, often with type prefix | `Stage_Vertex`, `VK_Poison` |
| Files | Match dominant project pattern | `MaterialIR.cpp` |

- Names must express intent; avoid `tmp`, `data2`, `doStuff`
- Prefer **verbs for functions**, **nouns for types**

### Class members

- **Non-static data members**: `m_` + **PascalCase** remainder

```cpp
class MIRBuilder
{
    const MaterialEdGraph* m_Graph = nullptr;
    MIREmitter* m_Emitter = nullptr;
    std::vector<MaterialGraphNodeDef*> m_RootNodeDefs;
};
```

### Singleton / class-level static instance

- **Static singleton (or shared class-level static state)**: `s_` + **PascalCase** remainder

```cpp
class Log
{
    static std::shared_ptr<spdlog::logger> s_CoreLogger;
    static std::shared_ptr<spdlog::logger> s_ClientLogger;
};
```

### Struct members

- **Plain `struct` data members**: **PascalCase**, **no** `m_` prefix

```cpp
struct MIRValue
{
    MIRValueKind Kind;
    const MIRValueType* Type;
};
```

- Use `class` + `m_` when encapsulation and invariants matter; use `struct` + bare PascalCase members for aggregates and IR-style PODs, consistent with the surrounding module.

## Smart Pointers (minEngine-style)

Follow minEngine patterns unless the project defines otherwise:

| Use case | Preference |
|----------|------------|
| Exclusive ownership inside a subsystem (graphs, builders, owned nodes) | `std::unique_ptr` + `std::make_unique` |
| Shared GPU / RHI resources, assets, loggers, anything with shared lifetime | `std::shared_ptr` + `std::make_shared` |
| Non-owning reference | Raw pointer or reference; document lifetime if non-obvious |
| Default for new code | `unique_ptr` unless shared ownership is required |

- No raw `new` / `delete` in new code
- Do not introduce `shared_ptr` only to avoid thinking about ownership

## Standard Library vs Project Types

- If the project has **no** custom container/string types and **no** mandated third-party replacement, use **`std` types** (`std::string`, `std::vector`, `std::unordered_map`, etc.)
- If the project provides types (e.g. engine string, handle types), use those in project code

## Special Member Functions

- Use **`= default`** and **`= delete`** actively:
  - `= default` for trivial special members when rule-of-zero does not apply
  - `= delete` for copying/moving when ownership or polymorphism forbids it
- Prefer **rule of zero** when all members already manage resources correctly

## `auto`

- **Low priority** when the right-hand side makes the type obvious
- If the type name is **short**, prefer spelling the full type
- Acceptable: verbose iterator types, complex `template` return types, structured bindings where names document intent

## Organization: Prefer Class Members Over Free Functions

- **Avoid** anonymous-namespace functions and file-scope `static` free functions when a **class member** (or `static` member) fits naturally
- **Prefer** `private` / `protected` member helpers to keep logic colocated with state
- **Exceptions** (free functions are fine):
  - Helpers for `enum` / `enum class` that do not belong on a single class
  - Factory functions, cast helpers, or algorithms shared across unrelated types
  - Wrapping in an extra class would be **more redundant** than a single free function
- If you **must** use an anonymous-namespace or file-scope `static` function **outside** these exceptions, **tell the user explicitly** why and what constraint forced it

## Error Handling & Exceptions

- **Handle errors locally** when you can (validate, log, return error code / `optional` / `bool` + `outError`)
- **Avoid throwing** when the failure can be resolved or reported at the call site without exceptions
- Do not use empty `catch (...)` blocks
- Use project assert / log macros (`check`, `ensure`, engine log) when available
- Document contracts for non-throwing APIs

## Headers

- Use `#pragma once` unless the project mandates include guards
- In `.h` / `.hpp`: **declarations only** — no non-trivial function bodies except small `inline`/`constexpr` helpers
- Include order (when no local style guide):
  - Matching header for `.cpp` (if any)
  - Project headers
  - Third-party headers
  - Standard library (`<...>`)
- Forward-declare when possible; include what you use in `.cpp`

## Comments & Documentation

- **Comments: English only**
- Explain *why*, not obvious *what*
- Brief API notes when behavior, preconditions, or ownership are not clear from the signature

## Formatting

- Match existing indentation and brace style in the same module
- If `.clang-format` exists, format edited code consistently
- Prefer wrapping over very long lines; follow project column limit if set

## Testing

- **No requirement** to add a test file for every new `.cpp`
- Add tests when the user asks or the project already has a testing convention for that area

## What to Avoid

- Large drive-by refactors unrelated to the task
- `using namespace std;` in headers
- C-style casts — use `static_cast`, `reinterpret_cast`, etc., deliberately
- `std::endl` in hot loops — prefer `'\n'` unless flush is required
- Heavy includes in widely included headers
- Non-English in source comments or in-code strings meant for maintainers
- New file-scope `static` / anonymous-namespace helpers without user-visible justification when a member function would work
