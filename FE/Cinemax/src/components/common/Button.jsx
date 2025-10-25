const Button = ({ children, onClick, variant = 'primary', disabled = false, className = '', type = 'button' }) => {
  const baseStyle = "px-4 py-2 font-semibold rounded-lg transition duration-150 ease-in-out shadow-md";
  let variantStyle = '';

  switch (variant) {
    case 'primary':
      variantStyle = 'bg-indigo-600 text-white hover:bg-indigo-700 disabled:bg-indigo-400';
      break;
    case 'secondary':
      variantStyle = 'bg-gray-200 text-gray-800 hover:bg-gray-300 disabled:bg-gray-100';
      break;
    case 'danger':
      variantStyle = 'bg-red-600 text-white hover:bg-red-700 disabled:bg-red-400';
      break;
    default:
      variantStyle = 'bg-indigo-600 text-white hover:bg-indigo-700 disabled:bg-indigo-400';
  }

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`${baseStyle} ${variantStyle} ${className}`}
    >
      {children}
    </button>
  );
};
export default Button;