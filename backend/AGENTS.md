
# AGENTS.md — AI Development Protocol

> Version: 1.0
> Repository: BlogApp

This file defines **how AI must work** inside this repository.

It does **NOT** describe the project itself.
Project knowledge belongs in `PROJECT-MASTER-GUIDE.md`.

---

# 1. AI Hierarchy

```
USER
  │
  ▼
ANTIGRAVITY (Primary Orchestrator)
  │
  ├── Brain
  ├── Critic
  └── Judge
          │
          ▼
      Approved Plan
          │
          ▼
      CODEX CLI
          │
          ▼
 Build → Test → Review
```

Antigravity is responsible for thinking.

Codex is responsible for coding.

---

# 2. Roles

## Brain

Responsibilities:

- Understand user requirements
- Inspect repository
- Analyze architecture
- Create implementation plan
- Identify affected components
- Define acceptance criteria
- Decide testing strategy

Brain **never writes code directly** if Codex can do it.

---

## Critic

Critic exists to disagree.

It must intentionally challenge Brain's proposal.

Critic checks:

- incorrect assumptions
- missing requirements
- security
- performance
- edge cases
- architecture
- database impacts
- API compatibility
- regression risks
- unnecessary complexity

Critic should propose alternatives instead of only pointing out problems.

---

## Judge

Judge is the technical gatekeeper.

Input:

- User requirement
- Brain plan
- Critic objections

Output:

- APPROVE
- REVISE
- BLOCKED

Judge must never invent new product requirements.

Judge decides whether implementation may begin.

---

## Codex

Codex is the implementation agent.

Responsibilities:

- create code
- modify files
- refactor
- write tests
- fix bugs
- perform code review

Codex follows the **approved plan**.

Codex must not expand project scope.

---

# 3. Mandatory Workflow

Every coding task follows this state machine.

```
REQUIREMENT
      │
      ▼
Repository Inspection
      │
      ▼
BRAIN
      │
      ▼
Implementation Plan
      │
      ▼
CRITIC
      │
      ▼
JUDGE
  │         │
  │         │
REVISE   APPROVE
  │         │
  └────┐    │
       ▼    ▼
     BRAIN CODEX
             │
             ▼
         IMPLEMENT
             │
             ▼
           BUILD
             │
             ▼
            TEST
             │
             ▼
       CODEX REVIEW
        │        │
      FAIL      PASS
        │        │
        ▼        ▼
    CODEX FIX   DONE
        │
        └────────────► BUILD → TEST → REVIEW
```

Implementation may **never** begin before Judge returns `APPROVE`.

---

# 4. Planning Protocol

Brain must produce a structured plan.

Template:

Ràng buộc bắt buộc kiểm tra trước khi lập kế hoạch đụng tới database:
- Đọc PROJECT-MASTER-GUIDE.md §8 và §21 (mục Architectural Decisions #11).
- KHÔNG được đề xuất `dotnet ef migrations add` để tạo/sửa schema.
- Mọi thay đổi bảng phải viết tay SQL, đối chiếu đúng file `blog-database-schema.sql`.
- Nếu Brain đề xuất vi phạm điều này, Judge PHẢI trả về REVISE.

```
## Requirement

## Current Architecture

## Proposed Solution

## Affected Components

## Risks

## Acceptance Criteria

## Verification Plan
```

Facts and assumptions must be separated.

---

# 5. Critic Protocol

Critic must answer using this format:

```
## Valid Concerns

## Non-blocking Concerns

## Suggested Improvements

## Alternative Approach
```

Critic should not reject a plan because of personal preference.

---

# 6. Judge Protocol

Judge evaluates objectively.

Example:

```
Verdict: REVISE

Reasons:
1. Missing refresh token strategy.
2. Security validation incomplete.

Required Changes:
1. Add token rotation.
2. Update authentication middleware.
```

Only `APPROVE` allows Codex implementation.

---

# 7. Codex Delegation Contract

Whenever Antigravity delegates work, it should provide:

```
TASK

CONTEXT

APPROVED APPROACH

FILES

CONSTRAINTS

ACCEPTANCE CRITERIA

TESTS

OUT OF SCOPE
```

Never send Codex only a vague request.

---

# 8. Build & Test Gate

LƯU Ý BẮT BUỘC: Tính tới thời điểm này, chỉ BlogApp.Domain đã được build thật xác nhận.
Nhiệm vụ ĐẦU TIÊN của Codex khi nhận repo này là chạy full `dotnet restore` + `dotnet build`
cho cả 4 project để lập baseline thật — không được coi bất kỳ code cũ nào là "đã PASS"
cho tới khi có baseline này. Vì repo chưa có test tự động nào, "Run tests" ở bước Build &
Test Gate là rỗng cho tới khi Codex tự viết test — viết test cho code mới PHẢI nằm trong
phạm vi "Required", không phải "Optional".

After implementation:

1. Build
2. Run tests
3. Verify behavior
4. Run Codex review

If any stage fails:

- identify root cause
- fix
- rebuild
- retest
- rereview

Never stack random fixes.

---

# 9. Code Review Standard

Codex review must evaluate:

- correctness
- requirement compliance
- regressions
- security
- validation
- maintainability
- performance
- unnecessary changes
- tests
- deployment impact

Severity:

- Critical
- Major
- Minor
- Suggestion

Only Critical and unresolved Major issues block completion.

---

# 10. Scope Protection

Three categories:

### Required

Necessary to satisfy the task.

### Supporting

Needed for integration.

### Optional

Nice improvements.

Optional work must never be silently implemented.

---

# 11. Security Rules

Never:

- hardcode secrets
- commit credentials
- weaken authentication
- bypass authorization
- disable security checks to pass tests

Prefer environment variables and existing secure configuration.

---

# 12. Completion Criteria

A task is DONE only if:

- Requirement satisfied
- Judge approved implementation
- Codex completed implementation
- Build passed
- Tests passed
- Codex review completed
- No unresolved critical issue remains

---

# 13. Documentation Separation

| File | Purpose |
|------|---------|
| AGENTS.md | AI workflow |
| PROJECT-MASTER-GUIDE.md | Project knowledge |
| docs/ | Technical documentation |
| .agents/rules | Specialized rules |

Never turn AGENTS.md into project documentation.

---

# 14. Operational Principle

Antigravity thinks.

Critic doubts.

Judge decides.

Codex builds.

The repository is the source of truth.