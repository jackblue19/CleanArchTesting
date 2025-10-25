"use client"

import { Button } from "@/components/ui/button"

interface MovieCardProps {
  movie: any
  isExpanded: boolean
  onExpand: () => void
  onSelectShowtime: (showtime: string) => void
}

export default function MovieCard({ movie, isExpanded, onExpand, onSelectShowtime }: MovieCardProps) {
  return (
    <div className="bg-card rounded-lg overflow-hidden border border-border hover:border-primary transition">
      <div className="relative">
        <img src={movie.image || "/placeholder.svg"} alt={movie.title} className="w-full h-64 object-cover" />
        <div className="absolute top-3 right-3 bg-primary text-primary-foreground px-3 py-1 rounded-full text-sm font-semibold">
          {movie.rating}
        </div>
      </div>

      <div className="p-4">
        <h3 className="text-lg font-bold text-foreground mb-1">{movie.title}</h3>
        <p className="text-sm text-muted-foreground mb-3">{movie.genre}</p>
        <p className="text-sm text-accent font-semibold mb-4">${movie.price}</p>

        <Button onClick={onExpand} variant="outline" className="w-full mb-3 bg-transparent">
          {isExpanded ? "Hide Showtimes" : "View Showtimes"}
        </Button>

        {isExpanded && (
          <div className="space-y-2 pt-3 border-t border-border">
            {movie.showtimes.map((showtime: string) => (
              <button
                key={showtime}
                onClick={() => onSelectShowtime(showtime)}
                className="w-full px-3 py-2 bg-secondary hover:bg-primary text-foreground rounded transition text-sm font-medium"
              >
                {showtime}
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
