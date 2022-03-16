import base64
import sys

if __name__ == '__main__':
    print ("Enter amount of chunks: ")
    amount_of_chunks = int(sys.stdin.readline())
    print ("Enter filename: ")
    target_file = sys.stdin.readline()

    with open(target_file, "wb") as f:
        for i in range(amount_of_chunks):
            print ("Paste chunk %s: " % (i+1))
            encoded_chunk = sys.stdin.readline()
            f.write(base64.standard_b64decode(encoded_chunk))