const mockShowtimes = [
  { id: 's1', movieId: 'm1', time: '19:00', theater: 'Rạp A', date: '2025-01-20', price: 100000 },
  { id: 's2', movieId: 'm1', time: '21:30', theater: 'Rạp B', date: '2025-01-20', price: 120000 },
  { id: 's3', movieId: 'm2', time: '18:00', theater: 'Rạp C', date: '2025-01-21', price: 90000 },
];

// Dữ liệu Ghế mô phỏng trạng thái ĐÃ ĐẶT (booked) và CÓ SẴN (available)
const mockSeats = {
  // Key là showtimeId, value là mảng trạng thái ghế
  's1': [
    { id: 'A1', status: 'available', type: 'standard' }, { id: 'A2', status: 'booked', type: 'standard' }, { id: 'A3', status: 'available', type: 'vip' },
    { id: 'B1', status: 'available', type: 'standard' }, { id: 'B2', status: 'available', type: 'standard' }, { id: 'B3', status: 'available', type: 'vip' },
    { id: 'C1', status: 'available', type: 'vip' }, { id: 'C2', status: 'booked', type: 'vip' }, { id: 'C3', status: 'available', type: 'vip' },
    // Thêm các ghế khác
    { id: 'A4', status: 'available', type: 'standard' }, { id: 'A5', status: 'available', type: 'standard' }, { id: 'A6', status: 'available', type: 'standard' }, { id: 'A7', status: 'available', type: 'standard' }, { id: 'A8', status: 'available', type: 'standard' },
    { id: 'B4', status: 'booked', type: 'standard' }, { id: 'B5', status: 'available', type: 'standard' }, { id: 'B6', status: 'available', type: 'standard' }, { id: 'B7', status: 'available', type: 'standard' }, { id: 'B8', status: 'available', type: 'standard' },
    { id: 'C4', status: 'available', type: 'vip' }, { id: 'C5', status: 'available', type: 'vip' }, { id: 'C6', status: 'available', type: 'vip' }, { id: 'C7', status: 'available', type: 'vip' }, { id: 'C8', status: 'available', type: 'vip' },
  ],
  's2': [
    { id: 'A1', status: 'available', type: 'standard' }, { id: 'A2', status: 'available', type: 'standard' }, { id: 'A3', status: 'available', type: 'standard' },
    { id: 'B1', status: 'available', type: 'standard' }, { id: 'B2', status: 'booked', type: 'standard' }, { id: 'B3', status: 'booked', type: 'standard' },
  ],
};


// Mô phỏng API call với độ trễ nhỏ
const mockApi = {
  getMovies: () => new Promise(resolve => setTimeout(() => resolve(mockMovies), 500)),
  getMovieDetail: (id) => new Promise(resolve => setTimeout(() => resolve(mockMovies.find(m => m.id === id)), 300)),
  getShowtimes: (movieId) => new Promise(resolve => setTimeout(() => resolve(mockShowtimes.filter(s => s.movieId === movieId)), 300)),
  getSeats: (showtimeId) => new Promise(resolve => setTimeout(() => resolve(mockSeats[showtimeId] || []), 300)),
  login: (username, password) => new Promise((resolve, reject) => {
    setTimeout(() => {
      if (username === 'admin' && password === '123') {
        resolve({ userId: 'u1', token: 'fake-admin-token', role: 'admin' });
      } else if (username === 'user' && password === '123') {
        resolve({ userId: 'u2', token: 'fake-user-token', role: 'user' });
      } else {
        reject('Thông tin đăng nhập không hợp lệ.');
      }
    }, 500);
  }),
};

export default mockApi;