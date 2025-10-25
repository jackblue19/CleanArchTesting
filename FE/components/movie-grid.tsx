"use client"

import { useState, useEffect } from "react"
import MovieCard from "./movie-card"
import { getShows } from "@/lib/api"

export default function MovieGrid({ onSelectMovie }: { onSelectMovie: (movie: any, showtime: string) => void }) {
  const [expandedMovie, setExpandedMovie] = useState<number | null>(null)
  const [movies, setMovies] = useState<any[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState("")

  useEffect(() => {
    const fetchMovies = async () => {
      try {
        const shows = await getShows()
        setMovies(shows)
      } catch (err) {
        setError("Failed to load movies")
        console.error("[v0] Error fetching shows:", err)
      } finally {
        setIsLoading(false)
      }
    }

    fetchMovies()
  }, [])

  if (isLoading) {
    return (
      <section className="max-w-7xl mx-auto px-4 py-12">
        <div className="text-center text-muted-foreground">Loading movies...</div>
      </section>
    )
  }

  if (error) {
    return (
      <section className="max-w-7xl mx-auto px-4 py-12">
        <div className="text-center text-red-500">{error}</div>
      </section>
    )
  }

  return (
    <section className="max-w-7xl mx-auto px-4 py-12">
      <div className="mb-8">
        <h2 className="text-3xl font-bold text-foreground mb-2">Now Showing</h2>
        <p className="text-muted-foreground">Select a movie and choose your preferred showtime</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {movies.map((movie) => (
          <MovieCard
            key={movie.showId}
            movie={movie}
            isExpanded={expandedMovie === movie.showId}
            onExpand={() => setExpandedMovie(expandedMovie === movie.showId ? null : movie.showId)}
            onSelectShowtime={(showtime) => {
              onSelectMovie(movie, showtime)
              setExpandedMovie(null)
            }}
          />
        ))}
      </div>
    </section>
  )
}
