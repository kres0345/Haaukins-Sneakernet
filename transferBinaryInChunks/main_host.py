import base64

def encode_binary(filePath: str):
    base64.b64encode()

    pass


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    amount_of_chunks = int(input("Enter amount of chunks: "))
    target_file = input("Enter filename: ")

    encoded_binary = ""
    for i in range(amount_of_chunks):
        encoded_chunk = input(f"Paste chunk {i}: ")
        encoded_binary += encoded_chunk

    with open(target_file, "wb") as f:
        f.write(base64.b64decode(encoded_binary))

