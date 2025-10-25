const SeatGrid = ({ seats, selectedSeats, onSelectSeat }) => {
  // Thay vì dùng rows/cols cố định, ta có thể tạo ra mảng từ seats
  // Tuy nhiên, vì đây là một component UI cho rạp, ta dùng fixed grid 8 cột
  const rows = ['A', 'B', 'C', 'D', 'E'];
  const cols = [1, 2, 3, 4, 5, 6, 7, 8];

  // Chuyển mảng tuyến tính thành bản đồ {id: seatObject} để dễ tra cứu
  const seatMap = seats.reduce((acc, seat) => {
    acc[seat.id] = seat;
    return acc;
  }, {});

  const getSeatState = (seatId) => {
    const seat = seatMap[seatId];
    if (!seat) return 'unavailable';

    if (seat.status === 'booked') {
      return 'booked';
    }
    if (selectedSeats.includes(seatId)) {
      return 'selected';
    }
    return seat.status; // available
  };

  const getSeatStyle = (state, type) => {
    const base = 'w-8 h-8 rounded-md m-1 flex items-center justify-center text-xs font-bold transition duration-150 select-none';
    if (state === 'booked') {
      return `${base} bg-red-400 text-white cursor-not-allowed opacity-70`;
    }
    if (state === 'selected') {
      return `${base} bg-green-500 text-white shadow-lg transform scale-105`;
    }
    // available
    const typeStyle = type === 'vip' ? 'bg-yellow-100 border-2 border-yellow-500 hover:bg-yellow-300' : 'bg-gray-200 hover:bg-gray-300';
    return `${base} text-gray-700 cursor-pointer ${typeStyle}`;
  };

  const handleSeatClick = (seatId) => {
    const seat = seatMap[seatId];
    if (!seat || seat.status === 'booked') return;
    onSelectSeat(seatId);
  };

  return (
    <div className="bg-white p-4 sm:p-6 rounded-xl shadow-2xl border border-gray-200">
      <div className="text-center mb-6 bg-gray-900 text-white py-3 rounded-xl font-mono tracking-widest text-lg shadow-inner">
        MÀN HÌNH CHIẾU
      </div>
      <div className="overflow-x-auto">
        <div className="flex justify-center flex-col items-start mx-auto w-fit">
          {rows.map((row) => (
            <div key={row} className="flex items-center">
              <span className="w-8 h-8 m-1 text-center font-bold text-gray-500 mr-2 flex-shrink-0">{row}</span>
              {cols.map((col) => {
                const seatId = `${row}${col}`;
                const seatType = seatMap[seatId]?.type || 'standard';
                const state = getSeatState(seatId);

                // Dùng padding cho mobile
                return (
                  <div
                    key={seatId}
                    title={`${seatId} (${state === 'booked' ? 'Đã đặt' : state === 'selected' ? 'Đang chọn' : seatType.toUpperCase()})`}
                    className={getSeatStyle(state, seatType)}
                    onClick={() => handleSeatClick(seatId)}
                  >
                    {col}
                  </div>
                );
              })}
            </div>
          ))}
        </div>
      </div>
      <div className="mt-8 flex flex-wrap justify-center gap-4 text-sm p-2 bg-gray-50 rounded-lg">
        <div className="flex items-center"><span className="w-4 h-4 rounded-full bg-green-500 mr-2"></span>Đang chọn</div>
        <div className="flex items-center"><span className="w-4 h-4 rounded-full bg-red-400 mr-2"></span>Đã đặt</div>
        <div className="flex items-center"><span className="w-4 h-4 rounded-full bg-gray-200 mr-2 border border-gray-400"></span>Thường</div>
        <div className="flex items-center"><span className="w-4 h-4 rounded-full bg-yellow-100 mr-2 border-2 border-yellow-500"></span>VIP</div>
      </div>
    </div>
  );
};
export default SeatGrid;