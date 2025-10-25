export default function Header() {
  return (
    <header className="border-b border-border bg-card">
      <div className="max-w-7xl mx-auto px-4 py-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-primary rounded-lg flex items-center justify-center">
              <span className="text-primary-foreground font-bold text-lg">🎬</span>
            </div>
            <h1 className="text-2xl font-bold text-foreground">CineBook</h1>
          </div>
          <nav className="flex items-center gap-6">
            <a href="#" className="text-muted-foreground hover:text-foreground transition">
              Home
            </a>
            <a href="#" className="text-muted-foreground hover:text-foreground transition">
              My Bookings
            </a>
            <button className="px-4 py-2 bg-primary text-primary-foreground rounded-lg hover:opacity-90 transition">
              Sign In
            </button>
          </nav>
        </div>
      </div>
    </header>
  )
}
