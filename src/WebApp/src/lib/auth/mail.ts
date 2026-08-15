import "dotenv/config"
import { createTransport } from "nodemailer"

const {
  MAILPIT_HOST,
  MAILPIT_PORT,
  MAILPIT_URI
} = process.env;

const transporter = createTransport({
  host: MAILPIT_HOST,
  port: MAILPIT_PORT ? parseInt(MAILPIT_PORT) : 465,
  secure: false
});

export type MailData = {
  from: string,
  to: string,
  subject: string,
  text: string,
  html: string
}

export async function sendMail(data: MailData) {
  const info = await transporter.sendMail({
    from: data.from,
    to: data.to,
    subject: data.subject,
    text: data.text,
    html: data.html
  });

  console.log("Message sent: %s", info.messageId);
}

await transporter.verify();
