export const CUSTOMER_STORAGE_KEY = 'customer';

export const getStoredCustomer = () => {
  const rawCustomer = localStorage.getItem(CUSTOMER_STORAGE_KEY);
  if (!rawCustomer) {
    return null;
  }

  try {
    return JSON.parse(rawCustomer);
  } catch {
    return null;
  }
};

export const saveStoredCustomer = (customer) => {
  localStorage.setItem(CUSTOMER_STORAGE_KEY, JSON.stringify(customer));
};

export const clearStoredCustomer = () => {
  localStorage.removeItem(CUSTOMER_STORAGE_KEY);
};

export const getCustomerAccessToken = () => {
  return getStoredCustomer()?.accessToken || '';
};
