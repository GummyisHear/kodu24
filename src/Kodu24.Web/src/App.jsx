import { Route, Routes } from 'react-router'
import GuestOnly from './components/GuestOnly.jsx'
import Layout from './components/Layout.jsx'
import HomePage from './pages/HomePage.jsx'
import ListingDetailPage from './pages/ListingDetailPage.jsx'
import ListingsPage from './pages/ListingsPage.jsx'
import LoginPage from './pages/LoginPage.jsx'
import NotFoundPage from './pages/NotFoundPage.jsx'
import RegisterPage from './pages/RegisterPage.jsx'

// Lehtede skeem: docs/disain.md
export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="listings" element={<ListingsPage />} />
        <Route path="listings/:id" element={<ListingDetailPage />} />
        <Route
          path="login"
          element={
            <GuestOnly>
              <LoginPage />
            </GuestOnly>
          }
        />
        <Route
          path="register"
          element={
            <GuestOnly>
              <RegisterPage />
            </GuestOnly>
          }
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
