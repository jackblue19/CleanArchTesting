"use client"

import type React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { login } from "@/lib/api"

interface LoginFormProps {
  onLoginSuccess: (userId: number, email: string) => void
}

export default function LoginForm({ onLoginSuccess }: LoginFormProps) {
  const [email, setEmail] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState("")

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")
    setIsLoading(true)

    try {
      const user = await login(email)
      onLoginSuccess(user.userId, user.email)
    } catch (err) {
      setError("Failed to login. Please try again.")
      console.error("[v0] Login error:", err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <div className="bg-card rounded-lg border border-border p-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">CineBook</h1>
          <p className="text-muted-foreground mb-8">Book your movie tickets online</p>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-2">Email Address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="w-full px-4 py-2 bg-secondary border border-border rounded-lg text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                placeholder="your@email.com"
              />
            </div>

            {error && <p className="text-red-500 text-sm">{error}</p>}

            <Button
              type="submit"
              disabled={isLoading}
              className="w-full bg-primary hover:opacity-90 text-primary-foreground py-2 rounded-lg font-semibold"
            >
              {isLoading ? "Logging in..." : "Login"}
            </Button>
          </form>

          <p className="text-xs text-muted-foreground text-center mt-6">
            Enter your email to login or create an account
          </p>
        </div>
      </div>
    </div>
  )
}
