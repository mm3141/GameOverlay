# GameOverlay

## Contribution rules
Good way to start is to fix one of the existing issue OR add a new issue ( if it's already not there ).

- Do not merge anything directly to master branch, create an MR

- Performance and number of offsets required to implement a feature is very very important to me so due to this reason your MR might be rejected or asked to be modified.

- If your MR or idea is rejected feel free to create a branch or fork this tool and do whatever you want.

- Only I will release the GH tool to OC website or discord thread.

### Pre-commit hooks
Since GitHub limits certain features for private repositories, we cannot restrict pushes to master.
We can still have this feature via [pre-commit](https://pre-commit.com/) hooks.

These hooks are local only, so if you want to use them, here's how you set them up.

```sh
pip install -U pre-commit
pre-commit install
```

Each time you try to commit the hooks specified in `.pre-commit-config.yml` will run.
You can also try running them without committing: 
```sh
pre-commit run --all-files
```


