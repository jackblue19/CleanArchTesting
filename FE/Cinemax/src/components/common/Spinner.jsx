import React from 'react'

const Spinner = () => (
      <div className="flex justify-center items-center p-8">
    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
    <span className="ml-3 text-indigo-600">Đang tải...</span>
  </div>
)

export default Spinner