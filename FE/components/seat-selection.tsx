"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { getSeatAvailability, holdSeats } from "@/lib/api"

interface SeatSelectionProps {
  movie: any
  showtime: string
  userId: number
  onSelectSeats: (seats: string[], holdId: number) => void
  onBack: () => void
}

export default function SeatSelection({ movie, showtime, userId, onSelectSeats, onBack }: SeatSelectionProps) {
  const [selectedSeats, setSelectedSeats] = useState<string[]>([])
  const [seatAvailability, setSeatAvailability] = useState<any[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState("")
  const [isHolding, setIsHolding] = useState(false)

  useEffect(() => {
    const fetchSeats = async () => {
      try {
        const seats = await getSeatAvailability(movie.showId)
        setSeatAvailability(seats)
      } catch (err) {
        setError("Failed to load seat availability")
        console.error("[v0] Error fetching seats:", err)
      } finally {
        setIsLoading(false)
      }
    }

    fetchSeats()
  }, [movie.showId])

  const getSeatStatus = (rowLabel: string, seatNumber: number) => {
    const seat = seatAvailability.find((s) => s.rowLabel === rowLabel && s.seatNumber === seatNumber)
    if (selectedSeats.includes(`${rowLabel}${seatNumber}`)) return "selected"
    if (seat?.seatState === "booked" || seat?.seatState === "held") return "booked"
    return "available"
  }

  const toggleSeat = (seatId: string) => {
    const status = getSeatStatus(seatId[0], Number.parseInt(seatId.slice(1)))
    if (status === "booked") return

    setSelectedSeats((prev) => (prev.includes(seatId) ? prev.filter((s) => s !== seatId) : [...prev, seatId]))
  }

  const handleContinue = async () => {
    setIsHolding(true)
    try {
      const seatIds = seatAvailability
        .filter((s) => selectedSeats.includes(`${s.rowLabel}${s.seatNumber}`))
        .map((s) => s.seatId)

      const holdResult = await holdSeats({
        showId: movie.showId,
        seatIds,
        userId,
      })

      onSelectSeats(selectedSeats, holdResult.holdId)
    } catch (err) {
      setError("Failed to hold seats")
      console.error("[v0] Error holding seats:", err)
    } finally {
      setIsHolding(false)
    }
  }

  const totalPrice = selectedSeats.length * movie.price
  const SEAT_ROWS = ["A", "B", "C", "D", "E", "F", "G", "H"]

  if (isLoading) {
    return (
      <section className="max-w-4xl mx-auto px-4 py-12">
        <div className="text-center text-muted-foreground">Loading seats...</div>
      </section>
    )
  }

  return (
    <section className="max-w-4xl mx-auto px-4 py-12">
      <button
        onClick={onBack}
        className="mb-6 text-muted-foreground hover:text-foreground transition flex items-center gap-2"
      >
        ← Back to Movies
      </button>

      <div className="bg-card rounded-lg border border-border p-8">
        <div className="mb-8">
          <h2 className="text-2xl font-bold text-foreground mb-2">{movie.movieTitle}</h2>
          <p className="text-muted-foreground">{showtime}</p>
        </div>

        <div className="mb-8">
          <div className="flex justify-center mb-8">
            <div className="text-center">
              <div className="w-full h-2 bg-gradient-to-r from-transparent via-primary to-transparent rounded-full mb-4"></div>
              <p className="text-sm text-muted-foreground">SCREEN</p>
            </div>
          </div>

          <div className="flex flex-col items-center gap-3 mb-8">
            {SEAT_ROWS.map((row) => (
              <div key={row} className="flex items-center gap-2">
                <span className="w-6 text-center text-sm font-semibold text-muted-foreground">{row}</span>
                <div className="flex gap-2">
                  {Array.from({ length: 12 }).map((_, i) => {
                    const seatId = `${row}${i + 1}`
                    const status = getSeatStatus(row, i + 1)

                    return (
                      <button
                        key={seatId}
                        onClick={() => toggleSeat(seatId)}
                        disabled={status === "booked"}
                        className={`w-8 h-8 rounded transition ${
                          status === "available"
                            ? "bg-secondary hover:bg-primary text-foreground cursor-pointer"
                            : status === "selected"
                              ? "bg-primary text-primary-foreground"
                              : "bg-muted text-muted-foreground cursor-not-allowed"
                        }`}
                        title={seatId}
                      >
                        <span className="text-xs font-semibold">{i + 1}</span>
                      </button>
                    )
                  })}
                </div>
              </div>
            ))}
          </div>

          <div className="flex justify-center gap-6 text-sm">
            <div className="flex items-center gap-2">
              <div className="w-4 h-4 bg-secondary rounded"></div>
              <span className="text-muted-foreground">Available</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-4 h-4 bg-primary rounded"></div>
              <span className="text-muted-foreground">Selected</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-4 h-4 bg-muted rounded"></div>
              <span className="text-muted-foreground">Booked</span>
            </div>
          </div>
        </div>

        {error && <p className="text-red-500 text-sm mb-4">{error}</p>}

        <div className="border-t border-border pt-6">
          <div className="flex justify-between items-center mb-6">
            <div>
              <p className="text-muted-foreground mb-1">Selected Seats</p>
              <p className="text-xl font-bold text-foreground">
                {selectedSeats.length > 0 ? selectedSeats.sort().join(", ") : "None"}
              </p>
            </div>
            <div className="text-right">
              <p className="text-muted-foreground mb-1">Total Price</p>
              <p className="text-2xl font-bold text-primary">${totalPrice.toFixed(2)}</p>
            </div>
          </div>

          <Button
            onClick={handleContinue}
            disabled={selectedSeats.length === 0 || isHolding}
            className="w-full bg-primary hover:opacity-90 text-primary-foreground py-3 rounded-lg font-semibold"
          >
            {isHolding ? "Holding seats..." : "Continue to Checkout"}
          </Button>
        </div>
      </div>
    </section>
  )
}
