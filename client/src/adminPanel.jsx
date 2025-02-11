import { NavLink } from "react-router";
import NavBar from "./NavBar";
import "./style/removeAddCustomerSupport.css"


function AddRemoveCustomerSupport() {
    return (
        <div className="sec">
            <div className="buttonContainer">
                <button onClick={addCustomer}>Add Customer Support</button>
                <button onClick={removeCustomer}>Remove Customer Support</button>
            </div>
        </div>
    )

}

function addCustomer() {
    console.log("hej")
    return (
        <form>
            <NavLink to="/adminPanel"><label>Email: <input type="text" name="email" /></label></NavLink>
        </form>
    )
}

function removeCustomer() {

}

export default AddRemoveCustomerSupport;