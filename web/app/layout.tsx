import type React from "react"
import "./globals.css"
import { Sidebar } from "@/components/sidebar"
import { ThemeProvider } from "@/components/theme-provider"

export const metadata = {
  title: "Patient Portal",
  description: "A modern patient portal for managing healthcare needs",
    generator: 'v0.dev'
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="bg-background min-h-screen">
        <ThemeProvider attribute="class" defaultTheme="light" enableSystem disableTransitionOnChange>
          <div className="flex min-h-screen flex-col md:flex-row">
            <Sidebar />
            <main className="flex-1 p-6 pt-0 md:p-8">{children}</main>
          </div>
        </ThemeProvider>
      </body>
    </html>
  )
}

import './globals.css'