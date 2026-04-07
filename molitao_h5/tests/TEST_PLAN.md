# Tests plan for chat group history and system channel handling

- Objective: ensure loadHistoryMessage error paths are handled gracefully, getGroupHistory rejects on error, and system channel handling is isolated.
- Scope:
  - groupChat.vue: loadHistoryMessage error path coverage
  - chatStore.ts: getGroupHistory error rejection, empty history initialization, and system channel merging behavior
  - system channel handling: ensure no duplicate system messages and correct merge ordering
- Approach:
  - Unit tests (where feasible) for pure functions and store methods with mocked api responses
  - Integration/End-to-end tests for UI error prompts and system-channel merging
- Suggested test cases:
  1. loadHistoryMessage: network error -> shows user toast and rejects
  2. getGroupHistory: API error -> rejects with error
  3. getGroupHistory: empty history with reload=true initializes chatMap key
  4. System channel: loading system history merges without duplicating existing system messages
- Metrics:
  - Coverage: aim for >70% on touched modules when tests exist
  - Performance: ensure no regressions in history loading latency
