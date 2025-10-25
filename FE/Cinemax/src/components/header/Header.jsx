const Header = ({ navigate }) => {
  const { user, logout } = useAuth();
  return (
    <header className="bg-white shadow-lg sticky top-0 z-10">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex justify-between items-center h-16">
        <div className="flex items-center space-x-6">
          <h1 className="text-2xl font-bold text-indigo-600 cursor-pointer" onClick={() => navigate('home')}>
            MovieApp
          </h1>
          <nav className="hidden md:flex space-x-4">
            <NavItem onClick={() => navigate('home')}>Phim Chiếu Rạp</NavItem>
            {user && user.role === 'admin' && (
                <NavItem onClick={() => navigate('admin')}>Quản Lý</NavItem>
            )}
          </nav>
        </div>
        <div>
          {user ? (
            <div className="flex items-center space-x-3">
              <span className="text-sm text-gray-600 hidden sm:inline">Xin chào, <span className="font-semibold">{user.userId}</span></span>
              <Button onClick={logout} variant="secondary">Đăng Xuất</Button>
            </div>
          ) : (
            <Button onClick={() => navigate('login')}>Đăng Nhập</Button>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;

const NavItem = ({ children, onClick }) => (
    <div onClick={onClick} className="text-gray-600 hover:text-indigo-600 cursor-pointer transition duration-150 text-sm py-1">
        {children}
    </div>
);


