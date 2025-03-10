import { useEffect, useState, useContext, createContext } from "react";

export default ChangePassword;

function ChangePassword() {
    const [checkPassword, setCheckPassword] = useState("");
    const [password, setPassword] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (checkPassword !== password) {
            setCheckPassword("");
            setPassword("");
            alert("The passwords don't match each other... Please try again");
            return;
        }


        try {
            const response = await fetch("", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ password }),
            });
            if (!response.ok) {
                throw new Error("Something went wrong with updating password");
            }
            alert("Password Changed!");
        } catch (error) {
            console.error(error);
            alert("Couldn't change password...");
        }
    }


    return <div>
        <form className="formContainer" onSubmit={handleSubmit}>
            <div className="formRow">
                <label>New Password: <input type="text" name="password1" required value={checkPassword}
                    onChange={(e) => setCheckPassword(e.target.value)}></input></label>
            </div>

            <div className="formRow">
                <label>Repeat New Password: <input type="text" name="password2" required value={password}
                    onChange={(e) => setPassword(e.target.value)}></input></label>
            </div>

            <div className="submitContainer">
                <button type="submit" id="sendButton">Send</button>
            </div>
        </form>
    </div>
}