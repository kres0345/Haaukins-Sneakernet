import base64
import pyclip

CHUNK_SIZE = 4032

if __name__ == '__main__':
    target_file = input("Enter filename: ")

    with open(target_file, "rb") as f:
        encoded_binary = []

        chunk = f.read(CHUNK_SIZE)
        while chunk != b'':
            encoded_binary.append(base64.b64encode(chunk).decode('ascii'))
            chunk = f.read(CHUNK_SIZE)

        chunk_count = len(encoded_binary)
        print(f"There are {chunk_count} chunks")

        i = 1
        for data_chunk in encoded_binary:
            pyclip.copy(data_chunk)
            print("..." + data_chunk[-10:])

            encoded_chunk = input(f"Chunk {i}/{chunk_count} copied to clipboard. Press enter to continue")
            i += 1
