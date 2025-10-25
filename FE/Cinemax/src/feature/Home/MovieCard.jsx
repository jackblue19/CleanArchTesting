const MovieCard = ({ movie, navigate }) => (
  <div className="bg-white rounded-xl shadow-lg hover:shadow-2xl transition duration-300 overflow-hidden transform hover:-translate-y-1">
    <img
      src={movie.poster}
      alt={movie.title}
      className="w-full h-80 object-cover"
      onError={(e) => { e.target.onerror = null; e.target.src = `https://placehold.co/300x450/4B5563/FFFFFF?text=${movie.title.replace(/\s/g, '+')}` }}
    />
    <div className="p-4">
      <h3 className="text-xl font-bold text-gray-800 mb-1 truncate">{movie.title}</h3>
      <p className="text-sm text-indigo-600 font-medium">{movie.genre}</p>
      <p className="text-xs text-gray-500 mt-2">Khởi chiếu: {movie.releaseDate}</p>
      <Button onClick={() => navigate(`detail/${movie.id}`)} className="mt-4 w-full">
        Xem Chi Tiết
      </Button>
    </div>
  </div>
);
export default MovieCard;