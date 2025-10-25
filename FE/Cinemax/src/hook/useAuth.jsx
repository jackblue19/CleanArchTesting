import { useState } from "react";
import { useContext } from "react";
import { createContext } from "react";

const AuthContext = createContext();

const useAuth = () => useContext(AuthContext);

const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(false);

  // Mô phỏng Toastify
  const showToast = (message, type = 'success') => {
    console.log(`[TOAST - ${type.toUpperCase()}]: ${message}`);
    // Ở đây bạn sẽ dùng toast.success(message) từ react-toastify
    const toastElement = document.getElementById('mock-toast');
    if(toastElement) {
        toastElement.textContent = `(${type.toUpperCase()}) ${message}`;
        toastElement.className = `fixed bottom-5 right-5 p-3 rounded-lg shadow-xl text-white z-50 transition-transform ${type === 'success' ? 'bg-indigo-600' : type === 'danger' ? 'bg-red-600' : 'bg-green-600'} translate-y-0`;
        setTimeout(() => {
             toastElement.className = toastElement.className.replace('translate-y-0', 'translate-y-[100px]');
        }, 3000);
    }
  };

  const login = async (username, password) => {
    setLoading(true);
    try {
      const result = await mockApi.login(username, password);
      setUser(result);
      showToast('Đăng nhập thành công!', 'success');
      return true;
    } catch (error) {
      showToast(error, 'error');
      return false;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    showToast('Đã đăng xuất.', 'success');
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, isAdmin: user && user.role === 'admin', showToast }}>
      {children}
      {/* Mô phỏng container của Toastify, thường nằm ở App.jsx ngoài cùng */}
      <div id="mock-toast" className="fixed bottom-[-100px] right-5 p-3 rounded-lg shadow-xl text-white z-50 transition-all duration-300 transform translate-y-[100px]"></div>
    </AuthContext.Provider>
  );
};
export { AuthProvider, useAuth };

