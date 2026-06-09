# Agent Entry Points

FE: SV22T1020320.Web/AI_PROJECT_CONTEXT.md
BE: AI_BE_CONTEXT.md

Semble:
- top_k = 3
- tối đa 2 search + 1 find_related

Không đọc:
- ARCHITECTURE.md full
- src/modules/chat/pages/ChatPage.tsx
- node_modules
- dist

---

# Git Workflow

Before any commit or push:

1. Check current branch.

2. Never commit directly to:
   - main
   - master

3. If current branch is main:
   create a new branch:

   feature/<feature-name>
   fix/<bug-name>
   refactor/<topic>

4. Use Conventional Commits:

   feat:
   fix:
   refactor:
   docs:
   test:
   chore:

5. Before pushing:
   - show changed files
   - summarize modifications
   - show commit message

6. Push only to the feature branch.

7. Never:
   - merge into main
   - create PR automatically
   - force push
   - delete branches

8. After push provide:
   - branch name
   - commit hash
   - commit message

9. Wait for human review before any merge.