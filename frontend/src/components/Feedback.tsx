export function ErrorBanner({ message }: { message: string }) {
  return <div className="banner banner-error">{message}</div>;
}

export function Spinner() {
  return <div className="spinner" role="status" aria-label="Loading" />;
}

export function EmptyState({ message }: { message: string }) {
  return <div className="empty-state">{message}</div>;
}
