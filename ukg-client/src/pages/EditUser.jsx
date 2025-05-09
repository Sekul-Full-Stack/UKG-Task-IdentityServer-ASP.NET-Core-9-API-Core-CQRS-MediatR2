import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom'; 
import styled from 'styled-components';
import { useUpdateUserMutation } from '../redux/services/apiSlice';

const FormWrapper = styled.div`
  max-width: 500px;
  margin: 40px auto;
  padding: 30px;
  background-color: #fff;
  border-radius: 10px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
`;

const Input = styled.input`
  display: block;
  width: 100%;
  padding: 12px;
  margin: 10px 0 20px;
  font-size: 16px;
`;

const Button = styled.button`
  padding: 12px 20px;
  background-color: rgb(10, 86, 60);
  color: white;
  border: none;
  cursor: pointer;
  font-size: 16px;
`;

const ErrorMsg = styled.p`
  color: red;
`;

const EditUser = () => {
    const { state: user } = useLocation();  
    const navigate = useNavigate();
    const [updateUser, { isLoading: isUpdating }] = useUpdateUserMutation();
  
    const [formData, setFormData] = useState({
      id: user?.Id || '',
      email: user?.Email || '',
      userName: user?.UserName || '',
      phoneNumber: user?.PhoneNumber || ''
    });
  
    const [error, setError] = useState('');
  
    const handleChange = (e) => {
      const { name, value } = e.target;
      setFormData((prev) => ({ ...prev, [name]: value }));
    };
  
    const handleSubmit = async (e) => {
      e.preventDefault();
      setError('');
  
      try {
        await updateUser(formData).unwrap();
        navigate('/users');
      } catch (err) {
        setError('Failed to update user.');
      }
    };
  
    if (!user) return <ErrorMsg>No user data provided.</ErrorMsg>;
  
    return (
      <FormWrapper>
        <h2>Edit User</h2>
        {error && <ErrorMsg>{error}</ErrorMsg>}
        <form onSubmit={handleSubmit}>
          <label>Email</label>
          <Input type="email" name="email" value={formData.email} onChange={handleChange} required />
  
          <label>Username</label>
          <Input type="text" name="userName" disabled  value={formData.userName} onChange={handleChange} required />
  
          <label>Phone Number</label>
          <Input type="text" name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} required />
  
          <Button type="submit" disabled={isUpdating}>
            {isUpdating ? 'Updating...' : 'Update User'}
          </Button>
        </form>
      </FormWrapper>
    );
  };

export default EditUser;
