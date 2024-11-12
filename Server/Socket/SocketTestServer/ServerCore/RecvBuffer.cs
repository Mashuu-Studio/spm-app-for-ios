using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos = 0; // 데이터 처리를 위한 커서
        int _writePos = 0; // 패킷이 들어왔을 때 얼마나 들어왔는지 확인하는 커서
        // 5바이트만큼의 패킷이 들어오면 그만큼의 패킷을 처리할 수 있는 상태가 되는 것.
        // 뒤이어 다음패킷을 받아야하기 때문에 writePos를 5만큼 이동.
        // readPos는 0에 있다가 들어온 패킷을 처리한 뒤에 5로 이동.
        // 만약 처리해야할 패킷이 더 있다면 readPos는 대기.

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } } // 처리되지 않은 데이터의 크기.
        public int FreeSize { get { return _buffer.Count - _writePos; } } // 더 읽을 수 있는 남은 버퍼의 크기

        public ArraySegment<byte> ReadSegment // 처리할 수 있는 데이터의 유효범위
        {
            get
            {
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);
            }
        }
        public ArraySegment<byte> WriteSegment // 사용할 수 있는 데이터의 유효범위
        {
            get
            {
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
            }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            // 데이터처리가 전부 완료됐다면 초기화시켜줌.
            if (dataSize == 0)
            {
                // 남은 데이터가 없다면 복사 X
                _readPos = _writePos = 0; 
            }
            // 남은 데이터가 존재한다면 시작 위치로 복사.
            else
            {
                // 현재 들고있는 양 만큼 복사. writePos 뒷부분은 어차피 아직 읽어들이지 않은 부분임.
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }
        
        public bool OnRead(int numOfBytes)
        {
            // 처리량이 DataSize보다 크면 문제가 있음.
            if (numOfBytes > DataSize) return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            // 읽은 양이 FreeSize보다 크면 문제가 있음. (남은 것보다 더 읽었다는 뜻)
            if (numOfBytes > FreeSize) return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
