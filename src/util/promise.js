export const asCancelable = promise => {
    let isCanceled = false;

    const promiseWrapper = new Promise((resolve, reject) => {
        promise
            .then(res => isCanceled ? reject({ isCanceled: true }) : resolve(res))
            .catch(err => isCanceled ? reject({ isCanceled: true }) : reject(err));
    });

    return {
        promise: promiseWrapper,
        cancel() {
            isCanceled = true;
        }
    };
};
