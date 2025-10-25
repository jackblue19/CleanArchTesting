"use client"

import { useState } from "react"
import Header from "@/components/header"
import LoginForm from "@/components/login-form"
import MovieGrid from "@/components/movie-grid"
import SeatSelection from "@/components/seat-selection"
import Checkout from "@/components/checkout"

type BookingStep = "login" | "movies" | "seats" | "checkout"

export default function Home() {
  const [currentStep, setCurrentStep] = useState<BookingStep>("login")
  const [userId, setUserId] = useState<number | null>(null)
  const [userEmail, setUserEmail] = useState<string>("")
  const [selectedMovie, setSelectedMovie] = useState<any>(null)
  const [selectedSeats, setSelectedSeats] = useState<string[]>([])
  const [selectedShowtime, setSelectedShowtime] = useState<string>("")
  const [holdId, setHoldId] = useState<number | null>(null)

  const handleLoginSuccess = (id: number, email: string) => {
    setUserId(id)
    setUserEmail(email)
    setCurrentStep("movies")
  }

  const handleMovieSelect = (movie: any, showtime: string) => {
    setSelectedMovie(movie)
    setSelectedShowtime(showtime)
    setCurrentStep("seats")
  }

  const handleSeatsSelect = (seats: string[], holdId: number) => {
    setSelectedSeats(seats)
    setHoldId(holdId)
    setCurrentStep("checkout")
  }

  const handleBackToMovies = () => {
    setCurrentStep("movies")
    setSelectedMovie(null)
    setSelectedSeats([])
  }

  const handleBackToSeats = () => {
    setCurrentStep("seats")
    setSelectedSeats([])
  }

  const handleBookingSuccess = () => {
    setCurrentStep("movies")
    setSelectedMovie(null)
    setSelectedSeats([])
    setHoldId(null)
  }

  return (
    <main className="min-h-screen bg-background">
      {currentStep === "login" && <LoginForm onLoginSuccess={handleLoginSuccess} />}

      {currentStep !== "login" && (
        <>
          <Header />

          {currentStep === "movies" && <MovieGrid onSelectMovie={handleMovieSelect} />}

          {currentStep === "seats" && selectedMovie && userId && (
            <SeatSelection
              movie={selectedMovie}
              showtime={selectedShowtime}
              userId={userId}
              onSelectSeats={handleSeatsSelect}
              onBack={handleBackToMovies}
            />
          )}

          {currentStep === "checkout" && selectedMovie && userId && holdId && (
            <Checkout
              movie={selectedMovie}
              showtime={selectedShowtime}
              seats={selectedSeats}
              userId={userId}
              holdId={holdId}
              onBack={handleBackToSeats}
              onSuccess={handleBookingSuccess}
            />
          )}
        </>
      )}
    </main>
  )
}
