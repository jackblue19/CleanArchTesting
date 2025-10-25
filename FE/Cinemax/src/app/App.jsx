import { useState } from "react";
import React from "react";
import HomePage from "./pages/HomePage";
import DetailPage from "./pages/DetialPage";
import BookingPage from "./pages/BookingPage";
import LoginPage from "./pages/LoginPage";
import { AuthProvider } from "../hook/useAuth";
import AdminMoviePage from "./pages/admin/AdminMoviePage";
import AdminDashboard from "./pages/admin/AdminDashboard";

const App = () => {
  const [currentPath, setCurrentPath] = useState('home');

  const navigate = (path) => {
    setCurrentPath(path);
    window.scrollTo(0, 0); // Mô phỏng scroll top on route change
  };

  // Logic phân tách path và params (Mô phỏng useParams())
  const pathParts = currentPath.split('/');
  const routeBase = pathParts[0];
  const routeParam = pathParts[1];

  // Render component dựa trên path
  const renderRoute = () => {
    switch (routeBase) {
      case 'home':
        return <HomePage navigate={navigate} />;
      case 'detail':
        return <DetailPage navigate={navigate} movieId={routeParam} />;
      case 'booking':
        // Đảm bảo showtimeId được truyền
        if (!routeParam) return <div className="p-10 text-red-600 font-bold text-center">Lịch chiếu không hợp lệ.</div>;
        return <BookingPage navigate={navigate} showtimeId={routeParam} />;
      case 'login':
        return <LoginPage navigate={navigate} />;
      case 'admin':
        if (!routeParam || routeParam === 'dashboard') {
            return <AdminDashboard navigate={navigate} />;
        }
        if (routeParam === 'movies') {
            return <AdminMoviePage navigate={navigate} />;
        }
        if (routeParam === 'showtimes') {
            return <div className="p-10 text-center">Tính năng Quản Lý Lịch Chiếu đang được phát triển...</div>;
        }
        return <div className="p-10">404 - Trang Admin không tồn tại</div>;
      default:
        return <HomePage navigate={navigate} />; // Fallback to home
    }
  };

  return (
    <AuthProvider>
      <div className="font-sans antialiased">
        {renderRoute()}
      </div>
    </AuthProvider>
  );
};
export default App;