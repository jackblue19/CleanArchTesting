const LoginPage = ({ navigate }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login, loading, user } = useAuth();

  React.useEffect(() => {
      if (user) {
          navigate('home');
      }
  }, [user, navigate]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const success = await login(username, password);
    if (success) {
      navigate('home');
    }
  };

  return (
    <AuthLayout>
      <h2 className="text-3xl font-bold text-center text-indigo-600 mb-6">Đăng Nhập</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Tên đăng nhập (user/admin)</label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="w-full p-3 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500"
            required
            placeholder='Ví dụ: user hoặc admin'
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Mật khẩu (123)</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full p-3 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500"
            required
            placeholder='Mật khẩu là 123'
          />
        </div>
        <Button type="submit" disabled={loading} className="w-full py-3 text-lg">
          {loading ? 'Đang xử lý...' : 'Đăng Nhập'}
        </Button>
      </form>
      <p className="text-xs text-gray-500 mt-4 text-center">Dùng tài khoản *user/123* để đặt vé hoặc *admin/123* để vào trang quản trị.</p>
    </AuthLayout>
  );
};

export default LoginPage;
