const BookingPage = ({ navigate, showtimeId }) => {
  const { user, showToast } = useAuth();
  
  // Lấy dữ liệu ghế từ Mock API
  const fetchSeats = React.useCallback(() => mockApi.getSeats(showtimeId), [showtimeId]);
  const { data: seats, loading, error } = useFetch(fetchSeats, []);
  
  const [selectedSeats, setSelectedSeats] = useState([]);

  // Lấy thông tin showtime (MOCK - thực tế cần API call)
  const currentShowtime = mockShowtimes.find(st => st.id === showtimeId);

  // Xử lý chuyển hướng nếu không tìm thấy showtime hoặc chưa đăng nhập
  React.useEffect(() => {
    if (!user) {
      showToast('Vui lòng đăng nhập để đặt vé.', 'error');
      navigate('login');
      return;
    }
    if (!currentShowtime) {
        showToast('Lịch chiếu không hợp lệ hoặc đã hết hạn.', 'error');
        navigate('home');
    }
  }, [user, currentShowtime, navigate, showToast]);

  if (!user || !currentShowtime) return <MainLayout navigate={navigate}><Spinner /></MainLayout>;
  
  const totalCost = selectedSeats.length * currentShowtime.price;

  const handleSeatSelect = (seatId) => {
    setSelectedSeats(prev =>
      prev.includes(seatId) ? prev.filter(id => id !== seatId) : [...prev, seatId]
    );
  };

  const handleConfirmBooking = () => {
    if (selectedSeats.length === 0) {
      showToast('Vui lòng chọn ít nhất một ghế.', 'error');
      return;
    }
    // Logic gọi API backend .NET 8.0 để tạo giao dịch
    showToast(`Đã đặt ${selectedSeats.length} vé thành công! Tổng cộng: ${totalCost.toLocaleString()} VNĐ.`, 'success');
    setSelectedSeats([]); // Reset
    navigate('home'); // Về trang chủ
  };

  return (
    <MainLayout navigate={navigate}>
      <h2 className="text-3xl font-extrabold text-gray-900 mb-6">
        Đặt Vé: Lịch Chiếu {currentShowtime.time} ({currentShowtime.date})
      </h2>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2">
          {loading && <Spinner />}
          {error && <p className="text-red-500 text-center">Lỗi khi tải sơ đồ ghế.</p>}
          {/* Component SeatGrid (UI Friendly for Testing) */}
          {!loading && !error && seats.length > 0 && (
            <SeatGrid
                seats={seats}
                selectedSeats={selectedSeats}
                onSelectSeat={handleSeatSelect}
            />
          )}
        </div>
        <div className="lg:col-span-1 bg-white p-6 rounded-xl shadow-lg h-fit">
          <h3 className="text-2xl font-bold mb-4 border-b pb-3 text-indigo-700">Tóm Tắt Đơn Hàng</h3>
          <p className="text-lg mb-2">Số lượng ghế: <span className="font-bold text-indigo-600">{selectedSeats.length}</span></p>
          <div className="mb-2 p-2 bg-gray-50 rounded-lg max-h-40 overflow-y-auto">
            <p className="text-sm font-semibold text-gray-700">Ghế đã chọn:</p>
            <p className="font-mono text-sm break-words mt-1">{selectedSeats.join(', ') || 'Chưa chọn ghế nào'}</p>
          </div>
          <p className="text-lg mb-2">Giá mỗi vé: <span className="font-bold">{currentShowtime.price.toLocaleString()} VNĐ</span></p>
          <div className="border-t mt-4 pt-4">
            <p className="text-2xl font-extrabold text-red-600">
              Tổng Cộng: {totalCost.toLocaleString()} VNĐ
            </p>
          </div>
          <Button
            onClick={handleConfirmBooking}
            disabled={selectedSeats.length === 0}
            className="mt-6 w-full py-3 text-lg"
          >
            Xác Nhận Đặt Vé
          </Button>
        </div>
      </div>
    </MainLayout>
  );
};
export default BookingPage;