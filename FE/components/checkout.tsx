"use client"

import type React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { confirmBooking } from "@/lib/api"

interface CheckoutProps {
  movie: any
  showtime: string
  seats: string[]
  userId: number
  holdId: number
  onBack: () => void
  onSuccess: () => void
}

export default function Checkout({ movie, showtime, seats, userId, holdId, onBack, onSuccess }: CheckoutProps) {
  const [email, setEmail] = useState("")
  const [fullName, setFullName] = useState("")
  const [cardNumber, setCardNumber] = useState("")
  const [isProcessing, setIsProcessing] = useState(false)
  const [error, setError] = useState("")

  const subtotal = seats.length * movie.price
  const tax = subtotal * 0.08
  const total = subtotal + tax

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")
    setIsProcessing(true)

    try {
      await confirmBooking({
        holdId,
        userId,
        email,
      })

      alert(`Booking confirmed! Confirmation sent to ${email}`)
      onSuccess()
    } catch (err) {
      setError("Failed to complete booking. Please try again.")
      console.error("[v0] Booking error:", err)
    } finally {
      setIsProcessing(false)
    }
  }

  return (
    <section className="max-w-4xl mx-auto px-4 py-12">
      <button
        onClick={onBack}
        className="mb-6 text-muted-foreground hover:text-foreground transition flex items-center gap-2"
      >
        ← Back to Seat Selection
      </button>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Booking Summary */}
        <div className="lg:col-span-2">
          <div className="bg-card rounded-lg border border-border p-6 mb-6">
            <h2 className="text-2xl font-bold text-foreground mb-6">Order Summary</h2>

            <div className="space-y-4 mb-6 pb-6 border-b border-border">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Movie</span>
                <span className="text-foreground font-semibold">{movie.movieTitle}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Showtime</span>
                <span className="text-foreground font-semibold">{showtime}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Seats</span>
                <span className="text-foreground font-semibold">{seats.sort().join(", ")}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Quantity</span>
                <span className="text-foreground font-semibold">{seats.length} ticket(s)</span>
              </div>
            </div>

            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="text-foreground">${subtotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tax (8%)</span>
                <span className="text-foreground">${tax.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-lg font-bold pt-3 border-t border-border">
                <span className="text-foreground">Total</span>
                <span className="text-primary">${total.toFixed(2)}</span>
              </div>
            </div>
          </div>

          {/* Payment Form */}
          <div className="bg-card rounded-lg border border-border p-6">
            <h3 className="text-xl font-bold text-foreground mb-6">Payment Details</h3>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-foreground mb-2">Full Name</label>
                <input
                  type="text"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  required
                  className="w-full px-4 py-2 bg-secondary border border-border rounded-lg text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                  placeholder="John Doe"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-2">Email Address</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  className="w-full px-4 py-2 bg-secondary border border-border rounded-lg text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                  placeholder="john@example.com"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-2">Card Number</label>
                <input
                  type="text"
                  value={cardNumber}
                  onChange={(e) => setCardNumber(e.target.value.replace(/\s/g, "").replace(/(.{4})/g, "$1 "))}
                  required
                  maxLength={19}
                  className="w-full px-4 py-2 bg-secondary border border-border rounded-lg text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                  placeholder="1234 5678 9012 3456"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">Expiry Date</label>
                  <input
                    type="text"
                    placeholder="MM/YY"
                    maxLength={5}
                    className="w-full px-4 py-2 bg-secondary border border-border rounded-lg text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-2">CVV</label>
                  <input
                    type="text"
                    placeholder="123"
                    maxLength={3}
                    className="w-full px-4 py-2 bg-secondary border border-border rounded-lg text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                  />
                </div>
              </div>

              {error && <p className="text-red-500 text-sm">{error}</p>}

              <Button
                type="submit"
                disabled={isProcessing}
                className="w-full bg-primary hover:opacity-90 text-primary-foreground py-3 rounded-lg font-semibold mt-6"
              >
                {isProcessing ? "Processing..." : `Complete Purchase - $${total.toFixed(2)}`}
              </Button>
            </form>
          </div>
        </div>

        {/* Order Summary Sidebar */}
        <div className="lg:col-span-1">
          <div className="bg-card rounded-lg border border-border p-6 sticky top-4">
            <h3 className="text-lg font-bold text-foreground mb-4">Booking Details</h3>

            <div className="space-y-3 text-sm">
              <div>
                <p className="text-muted-foreground mb-1">Movie</p>
                <p className="text-foreground font-semibold">{movie.movieTitle}</p>
              </div>

              <div>
                <p className="text-muted-foreground mb-1">Time</p>
                <p className="text-foreground font-semibold">{showtime}</p>
              </div>

              <div>
                <p className="text-muted-foreground mb-1">Seats</p>
                <div className="flex flex-wrap gap-2">
                  {seats.sort().map((seat) => (
                    <span
                      key={seat}
                      className="px-2 py-1 bg-primary text-primary-foreground rounded text-xs font-semibold"
                    >
                      {seat}
                    </span>
                  ))}
                </div>
              </div>

              <div className="pt-3 border-t border-border">
                <p className="text-muted-foreground mb-1">Total Amount</p>
                <p className="text-2xl font-bold text-primary">${total.toFixed(2)}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
