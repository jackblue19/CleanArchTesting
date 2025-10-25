const API_BASE_URL = "http://localhost:5228/api"

export interface Show {
  showId: number
  movieTitle: string
  genre: string
  rating: string
  image: string
  startTime: string
  price: number
}

export interface SeatAvailability {
  screenId: number
  showId: number
  seatId: number
  rowLabel: string
  seatNumber: number
  seatType: string
  seatState: "available" | "held" | "booked"
}

export interface HoldSeatsRequest {
  showId: number
  seatIds: number[]
  userId: number
}

export interface ConfirmBookingRequest {
  holdId: number
  userId: number
  email: string
}

export interface User {
  userId: number
  email: string
}

// Authentication
export async function login(email: string): Promise<User> {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email }),
  })

  if (!response.ok) throw new Error("Login failed")
  return response.json()
}

// Shows
export async function getShows(): Promise<Show[]> {
  const response = await fetch(`${API_BASE_URL}/shows`)
  if (!response.ok) throw new Error("Failed to fetch shows")
  return response.json()
}

// Seat Availability
export async function getSeatAvailability(showId: number): Promise<SeatAvailability[]> {
  const response = await fetch(`${API_BASE_URL}/shows/${showId}/availability`)
  if (!response.ok) throw new Error("Failed to fetch seat availability")
  return response.json()
}

export async function getOccupancy(showId: number) {
  const response = await fetch(`${API_BASE_URL}/shows/${showId}/occupancy`)
  if (!response.ok) throw new Error("Failed to fetch occupancy")
  return response.json()
}

// Booking
export async function holdSeats(req: HoldSeatsRequest) {
  const response = await fetch(`${API_BASE_URL}/booking/hold`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(req),
  })

  if (!response.ok) throw new Error("Failed to hold seats")
  return response.json()
}

export async function confirmBooking(req: ConfirmBookingRequest) {
  const response = await fetch(`${API_BASE_URL}/booking/confirm`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(req),
  })

  if (!response.ok) throw new Error("Failed to confirm booking")
  return response.json()
}

export async function releaseHold(holdId: number) {
  const response = await fetch(`${API_BASE_URL}/booking/release`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ holdId }),
  })

  if (!response.ok) throw new Error("Failed to release hold")
  return response.json()
}
