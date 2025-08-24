# Permissions Documentation Prompt Guide

Use these prompts to keep documentation and implementation in sync. Adapt role/permission names as needed.

## 1. Review Code vs Documentation
```
Analyze authorization for DOMAIN (e.g., credentials, admin actions). Compare policies and handlers in code with docs/permissions/DOMAIN.md and list discrepancies (missing rows/columns, incorrect grants, scope mismatches).
```

## 2. Update Documentation From Code
```
Read handlers and claim groups for DOMAIN and rewrite docs/permissions/DOMAIN.md matrix to reflect actual logic. Preserve formatting. Only change cells that differ. Explain changes succinctly at end.
```

## 3. Propose Implementation Changes To Match Documentation
```
Given docs/permissions/DOMAIN.md, list required code changes (new handler checks, claim group edits, policy additions) to make implementation conform. Provide patch-ready summaries per file.
```

## 4. Generate Code Patches
```
Using the previously listed required changes for DOMAIN, produce patches for affected files (AuthPolicies, handlers, BaseAuthorizationHelper) to enforce the documented matrix.
```

## 5. Validate After Changes
```
Run through each permission in docs/permissions/DOMAIN.md and describe test scenarios (user with claim X, resource Y) to assert matrix correctness. Flag any ambiguous cells needing integration tests.
```

## 6. Bulk Audit
```
For every file under docs/permissions/*.md (excluding prompt.md), produce a consolidated discrepancy report vs current code and rank fixes by risk.
```

## 7. Regenerate All Matrices
```
Scan all authorization handlers and claim groups; regenerate every docs/permissions/*.md matrix (except prompt.md) from scratch, overwriting current contents while preserving headings and notes when still accurate.
```

## 8. Suggest Tests
```
For DOMAIN, list unit test cases per policy to cover positive, negative, edge (ownership, wrong game, missing claim) paths derived from docs matrix.
```

## 9. Ownership / Resource Nuance Extraction
```
From code, enumerate which permissions depend on resource ownership and reflect that in the DOMAIN matrix using O markers; verify doc accuracy.
```

## 10. Role Impact Summary
```
Summarize effective capabilities for ROLE across all domains using the existing matrices. Highlight least-privilege adjustments.
```

---
Maintain consistent table formatting (pipes, header separator) for easy diffing.
