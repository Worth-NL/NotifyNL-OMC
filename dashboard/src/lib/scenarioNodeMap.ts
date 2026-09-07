/** Maps Mermaid node IDs (used in `click <id> go` directives) to scenario keys. */
export const SCENARIO_NODE_MAP: Record<string, string> = {
  routing: "routing",
  case_created: "case-created",
  case_updated: "case-updated",
  case_closed: "case-closed",
  task_assigned: "task-assigned",
  msg_received: "message-received",
  decision_made: "decision-made",
  kto: "kto",
};
