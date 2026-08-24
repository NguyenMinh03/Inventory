const STATUS_CLASSES: Record<string, string> = {
  Draft: "status-neutral",
  Sent: "status-info",
  Received: "status-active",
  Cancelled: "status-inactive",
};

export function StatusBadge({ status }: { status: string }) {
  return <span className={"status-badge " + (STATUS_CLASSES[status] ?? "status-neutral")}>{status}</span>;
}
