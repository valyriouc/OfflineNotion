use std::io::{Write, Read};

#[derive(Debug, Copy, Clone)]
pub enum PB {
    ENQ = 0x05, // Anfrage/Start
    RS = 0x1E, // Header start
    US = 0x1F
}

#[derive(Debug, Copy, Clone)]
pub enum POP {
    UPLOAD = 0x00,
    DOWNLOAD = 0x01
}

pub struct PHeader {
    key: String, 
    value: String
}

impl PHeader {
    pub fn new(key: String, value: String) -> Self {
        PHeader {
            key: key,
            value: value,
        }
    }

    pub fn read(stream: impl Read) -> Self {
        Self {
            key: String::from("Hello"),
            value: String::from("world"),
        }
    }

    pub fn write(&self, stream: &mut impl Write) {
        let key_b = self.key.as_bytes();
        let mut value_b = self.value.as_bytes();

        let len: i32 = i32::try_from(key_b.len() + 1 + value_b.len()).unwrap();

        let len_b = len.to_ne_bytes();
        stream.write_all(&len_b);
        stream.write_all(&key_b);
        
        let mut last = Vec::from("=".as_bytes());
        let mut tmp = Vec::from(value_b);
        last.append(&mut tmp);

        stream.write_all(&tmp);
    }

}

pub struct PMessage {
    operation: POP,
    headers: Vec<PHeader>,
    body: String
}

impl PMessage {
    pub fn new(op: POP, h: Vec<PHeader>, b: String) -> Self {
        PMessage {
            operation: op,
            headers: h,
            body: b,
        }
    }

    pub fn read(stream: impl Read) -> Self {
        todo!();
    }

    pub fn write(&self, stream: &mut impl Write) {

        let start: [u8; 2] = [PB::ENQ as u8, self.operation as u8];
        stream.write_all(&start);

        for header in self.headers.iter().enumerate() {
            let hs: [u8; 1] = [PB::RS as u8];
            stream.write_all(&hs);
            header.1.write(stream);
        }

        let mut end: [u8; 1] = [PB::US as u8];

        let body = self.body.as_bytes();
        let len: i32 = i32::try_from(body.len()).unwrap();

        let len_b = len.to_ne_bytes();
        
        stream.write(&end);
        stream.write_all(&len_b);
        stream.write_all(&body);

        stream.flush();
    }
}